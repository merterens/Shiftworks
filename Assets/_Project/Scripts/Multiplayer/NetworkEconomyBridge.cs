using System;
using System.Collections.Generic;
using Shiftworks.Economy;
using Shiftworks.Machines;
using Shiftworks.SaveSystem;
using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// (Stage 11B) Makes the shared company wallet and machine purchase/placement
    /// host-authoritative, without touching EconomyManager's or MachinePurchaseManager's own
    /// TrySpend/TryPurchase/TryConsumePending — those stay the single, unchanged authority;
    /// this only decides WHO is allowed to call them directly (the server) and relays
    /// everyone else's requests to the server instead.
    ///
    /// Balance: NetworkVariable mirrors the server's own EconomyManager.Balance one-way,
    /// server -> everyone. Non-host clients apply it straight into their own local
    /// EconomyManager via RestoreBalance (the exact same entry point Stage 10's save/load
    /// uses), so GameHUD/MachineShopPanelUI need zero changes to display it correctly.
    ///
    /// Pending purchase counts: same one-way mirror, as a NetworkList indexed by position in
    /// SaveCatalog.AllMachines (reusing Stage 10's catalog instead of inventing a second
    /// machine registry). The shared pending pool is intentionally not per-player: any
    /// teammate can walk up and place a pending machine someone else's purchase paid for,
    /// same as the shared wallet funding it.
    ///
    /// Purchase/placement requests: the HOST path (IsServer) calls straight into
    /// MachinePurchaseManager synchronously, identical to single-player. A remote client's
    /// request goes through a ServerRpc; the host validates with the exact same
    /// TryPurchase/TryConsumePending call, then a targeted ClientRpc reports success/failure
    /// back to only that requester. Machines placed this way are also NetworkObject.Spawn'd
    /// so every client sees them appear — their production state (recipe progress, buffered
    /// input) is NOT yet synced, that is Stage 11C's scope.
    /// </summary>
    public class NetworkEconomyBridge : NetworkBehaviour
    {
        public static NetworkEconomyBridge Instance { get; private set; }

        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private MachinePurchaseManager purchaseManager;
        [SerializeField] private SaveCatalog catalog;

        private readonly NetworkVariable<int> _syncedBalance = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkList<int> _syncedPendingCounts = new NetworkList<int>();

        private Action<bool, string> _pendingPurchaseCallback;
        private Action<bool, string> _pendingPlacementCallback;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            _syncedBalance.OnValueChanged += HandleBalanceChanged;
            _syncedPendingCounts.OnListChanged += HandlePendingCountsChanged;

            if (IsServer)
            {
                _syncedBalance.Value = economyManager.Balance;
                InitializePendingCountsOnServer();
            }
            else
            {
                economyManager.RestoreBalance(_syncedBalance.Value);
                ApplyAllPendingCountsLocally();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            _syncedBalance.OnValueChanged -= HandleBalanceChanged;
            _syncedPendingCounts.OnListChanged -= HandlePendingCountsChanged;
        }

        private void Update()
        {
            // Polling rather than wrapping every EconomyManager call site: covers machine
            // purchases made here AND any other local spend/income this bridge doesn't know
            // about yet (F8 test money, order rewards, factory expansion/power purchases —
            // all still local-only per Stage 11B's scope), so the shared balance always
            // reflects whatever the host's own authoritative EconomyManager currently holds.
            if (!IsServer)
            {
                return;
            }

            if (economyManager.Balance != _syncedBalance.Value)
            {
                _syncedBalance.Value = economyManager.Balance;
            }
        }

        private void HandleBalanceChanged(int previousValue, int newValue)
        {
            if (IsServer)
            {
                return;
            }

            economyManager.RestoreBalance(newValue);
        }

        private void HandlePendingCountsChanged(NetworkListEvent<int> changeEvent)
        {
            if (IsServer)
            {
                return;
            }

            ApplyAllPendingCountsLocally();
        }

        private void InitializePendingCountsOnServer()
        {
            IReadOnlyList<MachineDefinition> allMachines = catalog != null ? catalog.AllMachines : null;
            int count = allMachines != null ? allMachines.Count : 0;

            _syncedPendingCounts.Clear();
            for (int i = 0; i < count; i++)
            {
                _syncedPendingCounts.Add(purchaseManager.GetPendingCount(allMachines[i]));
            }
        }

        private void ApplyAllPendingCountsLocally()
        {
            IReadOnlyList<MachineDefinition> allMachines = catalog != null ? catalog.AllMachines : null;
            if (allMachines == null)
            {
                return;
            }

            int count = Mathf.Min(allMachines.Count, _syncedPendingCounts.Count);
            for (int i = 0; i < count; i++)
            {
                purchaseManager.RestorePendingCount(allMachines[i], _syncedPendingCounts[i]);
            }
        }

        private int IndexOf(MachineDefinition definition)
        {
            IReadOnlyList<MachineDefinition> allMachines = catalog != null ? catalog.AllMachines : null;
            if (allMachines == null || definition == null)
            {
                return -1;
            }

            for (int i = 0; i < allMachines.Count; i++)
            {
                if (allMachines[i] == definition)
                {
                    return i;
                }
            }

            return -1;
        }

        private MachineDefinition ResolveDefinition(int catalogIndex)
        {
            IReadOnlyList<MachineDefinition> allMachines = catalog != null ? catalog.AllMachines : null;
            if (allMachines == null || catalogIndex < 0 || catalogIndex >= allMachines.Count)
            {
                return null;
            }

            return allMachines[catalogIndex];
        }

        private void AdjustPendingCount(int catalogIndex, int delta)
        {
            if (catalogIndex < 0 || catalogIndex >= _syncedPendingCounts.Count)
            {
                return;
            }

            _syncedPendingCounts[catalogIndex] = Mathf.Max(0, _syncedPendingCounts[catalogIndex] + delta);
        }

        private static ClientRpcParams TargetClient(ulong clientId) => new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };

        /// <summary>Called by MachineShopPanelUI instead of MachinePurchaseManager.TryPurchase
        /// directly. On the host this is synchronous (same as single-player); on a remote
        /// client it fires a ServerRpc and calls onResult once the host replies.</summary>
        public void RequestPurchase(MachineDefinition definition, Action<bool, string> onResult)
        {
            int index = IndexOf(definition);
            if (index < 0)
            {
                onResult?.Invoke(false, "Bilinmeyen makine (SaveCatalog'da yok)");
                return;
            }

            if (IsServer)
            {
                bool success = ExecutePurchaseOnServer(index);
                onResult?.Invoke(success, success ? "" : "Yetersiz bakiye!");
                return;
            }

            _pendingPurchaseCallback = onResult;
            RequestPurchaseServerRpc(index);
        }

        /// <summary>Called by MachinePlacementController instead of
        /// MachinePurchaseManager.TryConsumePending + Instantiate directly.</summary>
        public void RequestPlacement(MachineDefinition definition, Vector3 position, float rotationY, Action<bool, string> onResult)
        {
            int index = IndexOf(definition);
            if (index < 0)
            {
                onResult?.Invoke(false, "Bilinmeyen makine (SaveCatalog'da yok)");
                return;
            }

            if (IsServer)
            {
                bool success = ExecutePlacementOnServer(index, position, rotationY);
                onResult?.Invoke(success, success ? "" : "Bekleyen makine yok (az önce başka biri yerleştirmiş olabilir)");
                return;
            }

            _pendingPlacementCallback = onResult;
            RequestPlacementServerRpc(index, position, rotationY);
        }

        private bool ExecutePurchaseOnServer(int catalogIndex)
        {
            MachineDefinition definition = ResolveDefinition(catalogIndex);
            bool success = definition != null && purchaseManager.TryPurchase(definition);
            if (success)
            {
                AdjustPendingCount(catalogIndex, 1);
            }

            return success;
        }

        private bool ExecutePlacementOnServer(int catalogIndex, Vector3 position, float rotationY)
        {
            MachineDefinition definition = ResolveDefinition(catalogIndex);
            if (definition == null || !purchaseManager.TryConsumePending(definition))
            {
                return false;
            }

            AdjustPendingCount(catalogIndex, -1);

            GameObject placed = Instantiate(definition.MachinePrefab, position, Quaternion.Euler(0f, rotationY, 0f));
            MachineController placedController = placed.GetComponent<MachineController>();
            if (placedController != null)
            {
                placedController.MarkAsPlayerPlaced(Guid.NewGuid().ToString("N"), definition);
            }

            NetworkObject placedNetworkObject = placed.GetComponent<NetworkObject>();
            if (placedNetworkObject != null)
            {
                placedNetworkObject.Spawn(true);
            }

            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPurchaseServerRpc(int catalogIndex, ServerRpcParams rpcParams = default)
        {
            bool success = ExecutePurchaseOnServer(catalogIndex);
            NotifyPurchaseResultClientRpc(success, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPlacementServerRpc(int catalogIndex, Vector3 position, float rotationY, ServerRpcParams rpcParams = default)
        {
            bool success = ExecutePlacementOnServer(catalogIndex, position, rotationY);
            NotifyPlacementResultClientRpc(success, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ClientRpc]
        private void NotifyPurchaseResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Action<bool, string> callback = _pendingPurchaseCallback;
            _pendingPurchaseCallback = null;
            callback?.Invoke(success, success ? "" : "Yetersiz bakiye! (host onaylamadı)");
        }

        [ClientRpc]
        private void NotifyPlacementResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Action<bool, string> callback = _pendingPlacementCallback;
            _pendingPlacementCallback = null;
            callback?.Invoke(success, success ? "" : "Bekleyen makine yok (az önce başka biri yerleştirmiş olabilir)");
        }
    }
}
