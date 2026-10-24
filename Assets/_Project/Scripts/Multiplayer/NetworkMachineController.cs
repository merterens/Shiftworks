using Shiftworks.Items;
using Shiftworks.Machines;
using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// (Stage 11C bugfix) One-way host -> everyone mirror of a single MachineController's
    /// display state (State, buffered input count, processing elapsed time) — the exact
    /// same shape as NetworkOutputStorage's StoredCount mirror, just three fields instead of
    /// one. MachineController itself is untouched (TryAcceptInput/TryCollectOutput/
    /// CompleteProcessing all unchanged) — this only decides how a non-host client finds out
    /// what the host's own simulation is doing, via the new narrow RestoreDisplayState entry
    /// point (not the Stage 10 save-restore one, which would wrongly re-create the pending
    /// output locally on every client).
    ///
    /// The host's Update() here also does the one thing RestoreDisplayState deliberately
    /// does NOT: the moment the host's own machine transitions into OutputReady, the
    /// resulting item (already created by MachineController.CompleteProcessing — see
    /// PendingOutput) is NetworkObject.Spawn'd exactly once, so it appears identically for
    /// every client, not just the host. A client's own attempt to collect/feed is never
    /// routed through this component — that is NetworkItemInteractionBridge.
    ///
    /// Not synced: HasPower (kept false forever on a non-host client's own copy — this is
    /// what makes that copy's own Update() loop correctly stay inert for state progression,
    /// see MachineController.Update(): the Processing branch only advances _processingElapsed
    /// when HasPower is true, so a client's local simulation can never race the host's) and
    /// the stats fields (CompletedCount/TotalProcessingSeconds/TotalPowerWaitSeconds — not
    /// needed for the reported bug, out of scope for this fix).
    /// </summary>
    public class NetworkMachineController : NetworkBehaviour
    {
        [SerializeField] private MachineController machine;

        private readonly NetworkVariable<MachineState> _syncedState = new NetworkVariable<MachineState>(
            MachineState.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _syncedBufferedInputCount = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _syncedProcessingElapsed = new NetworkVariable<float>(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private MachineState _lastObservedStateOnServer;

        public override void OnNetworkSpawn()
        {
            if (machine == null)
            {
                machine = GetComponent<MachineController>();
            }

            _syncedState.OnValueChanged += HandleStateChanged;
            _syncedBufferedInputCount.OnValueChanged += HandleBufferedInputChanged;
            _syncedProcessingElapsed.OnValueChanged += HandleProcessingElapsedChanged;

            if (IsServer)
            {
                _lastObservedStateOnServer = machine.State;
                PushCurrentStateToClients();
            }
            else
            {
                ApplyLocally();
            }
        }

        public override void OnNetworkDespawn()
        {
            _syncedState.OnValueChanged -= HandleStateChanged;
            _syncedBufferedInputCount.OnValueChanged -= HandleBufferedInputChanged;
            _syncedProcessingElapsed.OnValueChanged -= HandleProcessingElapsedChanged;
        }

        private void Update()
        {
            if (!IsServer || machine == null)
            {
                return;
            }

            if (machine.State == MachineState.OutputReady && _lastObservedStateOnServer != MachineState.OutputReady)
            {
                SpawnPendingOutputIfNeeded();
            }

            _lastObservedStateOnServer = machine.State;
            PushCurrentStateToClients();
        }

        private void SpawnPendingOutputIfNeeded()
        {
            CarryableItem pending = machine.PendingOutput;
            if (pending == null)
            {
                return;
            }

            NetworkObject itemNetworkObject = pending.GetComponent<NetworkObject>();
            if (itemNetworkObject == null)
            {
                Debug.LogWarning($"[Shiftworks][MP] '{pending.name}' çıktı prefab'ında NetworkObject yok — " +
                    "sadece host'ta görünecek, diğer istemcilere yayılmayacak.");
                return;
            }

            if (!itemNetworkObject.IsSpawned)
            {
                itemNetworkObject.Spawn(true);
                Debug.Log($"[Shiftworks][MP] Makine çıktısı ağa açıldı: '{pending.name}' ({machine.name}).");
            }
        }

        private void PushCurrentStateToClients()
        {
            if (_syncedState.Value != machine.State)
            {
                _syncedState.Value = machine.State;
            }

            if (_syncedBufferedInputCount.Value != machine.BufferedInputCount)
            {
                _syncedBufferedInputCount.Value = machine.BufferedInputCount;
            }

            if (!Mathf.Approximately(_syncedProcessingElapsed.Value, machine.ProcessingElapsedSeconds))
            {
                _syncedProcessingElapsed.Value = machine.ProcessingElapsedSeconds;
            }
        }

        private void HandleStateChanged(MachineState previousValue, MachineState newValue)
        {
            if (!IsServer)
            {
                ApplyLocally();
            }
        }

        private void HandleBufferedInputChanged(int previousValue, int newValue)
        {
            if (!IsServer)
            {
                // (Stage 11C diagnostic) This is the PASSIVE sync path — fires only when the
                // host's own NetworkVariable actually changes value. If the Console/Player.log
                // shows the host accepting a feed (SERVER_REQUEST_ACCEPTED) but this line never
                // appears on the Client, the NetworkVariable write itself never reached this
                // client (a replication/observer problem, separate from the RPC round-trip).
                Debug.Log($"[Shiftworks][MP][FEED-SYNC] CLIENT_STATE_RECEIVED (NetworkVariable) — " +
                    $"makine={(machine != null ? machine.name : "null")}, " +
                    $"makineNetObjId={(NetworkObject != null ? NetworkObject.NetworkObjectId.ToString() : "yok")}, " +
                    $"BufferedInputCount: {previousValue} -> {newValue}, LocalClientId={NetworkManager.LocalClientId}.");
                ApplyLocally();
            }
        }

        private void HandleProcessingElapsedChanged(float previousValue, float newValue)
        {
            if (!IsServer)
            {
                ApplyLocally();
            }
        }

        private void ApplyLocally()
        {
            if (machine == null)
            {
                return;
            }

            machine.RestoreDisplayState(_syncedState.Value, _syncedBufferedInputCount.Value, _syncedProcessingElapsed.Value);
        }
    }
}
