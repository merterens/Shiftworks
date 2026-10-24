using Shiftworks.Conveyors;
using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// (Stage 11C bugfix) One-way host -> everyone mirror of OutputStorage.StoredCount —
    /// exactly the same shape as NetworkEconomyBridge's balance mirror, just scoped to this
    /// one scene singleton instead of EconomyManager. OutputStorage itself is untouched
    /// (stays a plain MonoBehaviour, TryAccept/TryWithdraw unchanged) — this only decides
    /// who may call TryWithdraw directly (the server) via NetworkItemInteractionBridge, and
    /// mirrors the resulting count through the existing Stage 10 RestoreStoredCount entry
    /// point so GameHUD/the storage's own InteractionPrompt need zero changes to show the
    /// correct shared number. Polling (not wrapping TryAccept/TryWithdraw) also means a
    /// future conveyor feeding this storage (Stage 11C-3, still local-only today) will
    /// already push its count out correctly with no further change here.
    /// </summary>
    public class NetworkOutputStorage : NetworkBehaviour
    {
        [SerializeField] private OutputStorage storage;

        private readonly NetworkVariable<int> _syncedStoredCount = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (storage == null)
            {
                storage = GetComponent<OutputStorage>();
            }

            _syncedStoredCount.OnValueChanged += HandleStoredCountChanged;

            if (IsServer)
            {
                _syncedStoredCount.Value = storage.StoredCount;
            }
            else if (storage != null)
            {
                storage.RestoreStoredCount(_syncedStoredCount.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            _syncedStoredCount.OnValueChanged -= HandleStoredCountChanged;
        }

        private void Update()
        {
            if (!IsServer || storage == null)
            {
                return;
            }

            if (storage.StoredCount != _syncedStoredCount.Value)
            {
                _syncedStoredCount.Value = storage.StoredCount;
            }
        }

        private void HandleStoredCountChanged(int previousValue, int newValue)
        {
            if (IsServer || storage == null)
            {
                return;
            }

            storage.RestoreStoredCount(newValue);
        }
    }
}
