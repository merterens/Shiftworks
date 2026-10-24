using System;
using System.Collections.Generic;
using Shiftworks.Economy;
using Shiftworks.Factory;
using Shiftworks.Machines;
using Shiftworks.Orders;
using Shiftworks.Power;
using Shiftworks.SaveSystem;
using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// (Stage 11B) Makes the shared company wallet, machine purchase/placement, factory
    /// expansion purchases, the power capacity upgrade, and which order is active
    /// host-authoritative — without touching any of EconomyManager/MachinePurchaseManager/
    /// FactoryExpansionManager/PowerManager/OrderManager's own atomic methods
    /// (TrySpend/TryPurchase/TryConsumePending/TryPurchaseUpgrade/TryAcceptOrder). Those stay
    /// the single, unchanged authority; this only decides WHO is allowed to call them
    /// directly (the server) and relays everyone else's requests to the server instead. The
    /// same four-part shape repeats for every one of these five systems: a one-way
    /// NetworkVariable/NetworkList mirror (server -> everyone, applied into the existing
    /// local manager through its existing RestoreX entry point), a synchronous host path, a
    /// ServerRpc for remote requests, and a targeted ClientRpc reporting the result back.
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
    ///
    /// Factory expansion: a NetworkList&lt;bool&gt; indexed by SaveCatalog.AllExpansions mirrors
    /// which expansions are unlocked; a client applies a newly-true entry via
    /// FactoryExpansionManager.RestoreUnlocked (already raises ExpansionUnlocked, so
    /// FactoryExpansionGate opens through its own existing, unchanged reaction path — zero
    /// gate/terminal changes needed).
    ///
    /// Power upgrade: a single NetworkVariable&lt;bool&gt; mirrors IsUpgradePurchased (there is
    /// only ever one upgrade tier today); applied via PowerManager.RestoreUpgradeState. Actual
    /// per-machine power ALLOCATION (UsedCapacityKw, TryRequestPower/ReleasePower) stays local
    /// per client — networking that is tied to production-state sync, Stage 11C's scope, same
    /// as a purchased machine's own recipe progress above.
    ///
    /// Active order: a NetworkVariable&lt;int&gt; mirrors the index (into OrderManager's own
    /// AvailableOrders list; -1 = none) of whichever order is Active/ReadyToShip anywhere,
    /// polled server-side so it also clears when an order is completed. (Stage 11C extension)
    /// Two more NetworkVariables — progress count and OrderState — are polled alongside it and
    /// applied together via OrderManager.RestoreOrderState, so a client's own "(N/M)"/"Paket
    /// Hazır" prompt now reflects the host's real packing progress instead of always reading
    /// a frozen 0/Active. Packing/shipping consumption itself is made host-authoritative by
    /// NetworkItemInteractionBridge.RequestPack/RequestDeliver (same shape as its existing
    /// RequestFeedMachine/RequestWithdraw) — this bridge only mirrors the resulting display
    /// state, same division of responsibility as NetworkMachineController/MachineController.
    /// </summary>
    public class NetworkEconomyBridge : NetworkBehaviour
    {
        public static NetworkEconomyBridge Instance { get; private set; }

        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private MachinePurchaseManager purchaseManager;
        [SerializeField] private SaveCatalog catalog;
        [SerializeField] private FactoryExpansionManager factoryExpansionManager;
        [SerializeField] private PowerManager powerManager;
        [SerializeField] private OrderManager orderManager;

        private readonly NetworkVariable<int> _syncedBalance = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkList<int> _syncedPendingCounts = new NetworkList<int>();
        private readonly NetworkList<bool> _syncedExpansionUnlocked = new NetworkList<bool>();

        private readonly NetworkVariable<bool> _syncedPowerUpgradePurchased = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _syncedActiveOrderIndex = new NetworkVariable<int>(
            -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // (Stage 11C extension) Mirror the active order's real packing progress/state —
        // previously ApplyActiveOrderLocally hardcoded (Active, 0), so a client's own
        // PackingStation prompt never advanced past "0/N" no matter how much was actually
        // packed on the host.
        private readonly NetworkVariable<int> _syncedActiveOrderProgress = new NetworkVariable<int>(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<OrderState> _syncedActiveOrderState = new NetworkVariable<OrderState>(
            OrderState.Available, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Action<bool, string> _pendingPurchaseCallback;
        private Action<bool, string> _pendingPlacementCallback;
        private Action<bool, string> _pendingExpansionCallback;
        private Action<bool, string> _pendingPowerUpgradeCallback;
        private Action<bool, string> _pendingOrderAcceptCallback;

        // (Stage 11B bugfix) Checked once, not re-checked (and not silently tolerated) every
        // frame: a missing reference here is a one-time SETUP mistake (menu 32 not (re-)run
        // after a field was added, or the manager it points at not existing in this scene
        // yet), not a transient runtime condition, so it is reported exactly once with the
        // exact field name(s) missing, and every public entry point (Update's polling
        // included) short-circuits cleanly afterwards instead of throwing a
        // NullReferenceException on every single Update() call forever.
        private bool _referencesValid;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            ResolveMissingReferences();
            _referencesValid = ValidateReferences();

            _syncedBalance.OnValueChanged += HandleBalanceChanged;
            _syncedPendingCounts.OnListChanged += HandlePendingCountsChanged;
            _syncedExpansionUnlocked.OnListChanged += HandleExpansionStateChanged;
            _syncedPowerUpgradePurchased.OnValueChanged += HandlePowerUpgradeChanged;
            _syncedActiveOrderIndex.OnValueChanged += HandleActiveOrderIndexChanged;
            _syncedActiveOrderProgress.OnValueChanged += HandleActiveOrderProgressChanged;
            _syncedActiveOrderState.OnValueChanged += HandleActiveOrderStateChanged;

            if (!_referencesValid)
            {
                return;
            }

            if (IsServer)
            {
                _syncedBalance.Value = economyManager.Balance;
                InitializePendingCountsOnServer();
                InitializeExpansionStateOnServer();
                _syncedPowerUpgradePurchased.Value = powerManager.IsUpgradePurchased;
                _syncedActiveOrderIndex.Value = IndexOfActiveOrder();
                PushActiveOrderProgressAndState();
            }
            else
            {
                economyManager.RestoreBalance(_syncedBalance.Value);
                ApplyAllPendingCountsLocally();
                ApplyAllExpansionStateLocally();
                powerManager.RestoreUpgradeState(_syncedPowerUpgradePurchased.Value);
                ApplyActiveOrderLocally(_syncedActiveOrderIndex.Value);
            }
        }

        /// <summary>(Stage 11B bugfix) Same fallback-lookup pattern MachineController.Awake()
        /// already uses for its own PowerManager reference: if the Editor setup menu that
        /// wires these fields was never (re-)run after a field was added — exactly what
        /// caused the reported NullReferenceException spam, confirmed by reading the scene's
        /// own serialized data, which had only the three original fields and none of the
        /// three added for this stage's extension — each scene has exactly one of every
        /// manager type here, so a one-time FindFirstObjectByType is a safe, correct
        /// self-heal instead of requiring the menu to be re-run before this bridge can work.</summary>
        private void ResolveMissingReferences()
        {
            if (economyManager == null) economyManager = FindFirstObjectByType<EconomyManager>();
            if (purchaseManager == null) purchaseManager = FindFirstObjectByType<MachinePurchaseManager>();
            if (catalog == null) catalog = FindFirstObjectByType<SaveCatalog>();
            if (factoryExpansionManager == null) factoryExpansionManager = FindFirstObjectByType<FactoryExpansionManager>();
            if (powerManager == null) powerManager = FindFirstObjectByType<PowerManager>();
            if (orderManager == null) orderManager = FindFirstObjectByType<OrderManager>();
        }

        /// <summary>Logs exactly which [SerializeField] reference(s) are missing, once, by
        /// name — not just "something is null" — so the fix is a one-line Inspector/Editor-menu
        /// action instead of a debugging session. Returns false if anything required is
        /// unset.</summary>
        private bool ValidateReferences()
        {
            List<string> missing = new List<string>();
            if (economyManager == null) missing.Add(nameof(economyManager));
            if (purchaseManager == null) missing.Add(nameof(purchaseManager));
            if (catalog == null) missing.Add(nameof(catalog));
            if (factoryExpansionManager == null) missing.Add(nameof(factoryExpansionManager));
            if (powerManager == null) missing.Add(nameof(powerManager));
            if (orderManager == null) missing.Add(nameof(orderManager));

            if (missing.Count == 0)
            {
                return true;
            }

            Debug.LogError("[Shiftworks][MP] NetworkEconomyBridge yapılandırması eksik — şu alan(lar) " +
                $"bağlı değil ve sahnede FindFirstObjectByType ile de bulunamadı: {string.Join(", ", missing)}. " +
                "Bu köprü devre dışı kalacak (ekonomi/makine/genişletme/elektrik/sipariş senkronizasyonu " +
                "çalışmayacak). Düzeltme: WorkshopMultiplayer.unity'de ilgili manager GameObject'in " +
                "gerçekten var olduğunu doğrulayın, gerekirse 'Shiftworks/Setup/32. Add Network Economy " +
                "Bridge To WorkshopMultiplayer Scene' menüsünü çalıştırıp yeni bir Windows build alın.");
            return false;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            _syncedBalance.OnValueChanged -= HandleBalanceChanged;
            _syncedPendingCounts.OnListChanged -= HandlePendingCountsChanged;
            _syncedExpansionUnlocked.OnListChanged -= HandleExpansionStateChanged;
            _syncedPowerUpgradePurchased.OnValueChanged -= HandlePowerUpgradeChanged;
            _syncedActiveOrderIndex.OnValueChanged -= HandleActiveOrderIndexChanged;
            _syncedActiveOrderProgress.OnValueChanged -= HandleActiveOrderProgressChanged;
            _syncedActiveOrderState.OnValueChanged -= HandleActiveOrderStateChanged;
        }

        private void HandleBalanceChanged(int previousValue, int newValue)
        {
            if (IsServer || !_referencesValid)
            {
                return;
            }

            economyManager.RestoreBalance(newValue);
        }

        private void HandlePendingCountsChanged(NetworkListEvent<int> changeEvent)
        {
            if (IsServer || !_referencesValid)
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

        // ---- Factory expansion (Stage 11B extension) ----

        private void HandleExpansionStateChanged(NetworkListEvent<bool> changeEvent)
        {
            if (IsServer || !_referencesValid)
            {
                return;
            }

            ApplyAllExpansionStateLocally();
        }

        private void InitializeExpansionStateOnServer()
        {
            IReadOnlyList<FactoryExpansionDefinition> allExpansions = catalog != null ? catalog.AllExpansions : null;
            int count = allExpansions != null ? allExpansions.Count : 0;

            _syncedExpansionUnlocked.Clear();
            for (int i = 0; i < count; i++)
            {
                _syncedExpansionUnlocked.Add(factoryExpansionManager.IsUnlocked(allExpansions[i]));
            }
        }

        private void ApplyAllExpansionStateLocally()
        {
            IReadOnlyList<FactoryExpansionDefinition> allExpansions = catalog != null ? catalog.AllExpansions : null;
            if (allExpansions == null)
            {
                return;
            }

            int count = Mathf.Min(allExpansions.Count, _syncedExpansionUnlocked.Count);
            for (int i = 0; i < count; i++)
            {
                // RestoreUnlocked no-ops if already unlocked, so re-applying every entry on
                // every change is safe — it also raises ExpansionUnlocked for a newly-true
                // entry, which is what makes FactoryExpansionGate open with zero gate changes.
                if (_syncedExpansionUnlocked[i])
                {
                    factoryExpansionManager.RestoreUnlocked(allExpansions[i]);
                }
            }
        }

        private int IndexOfExpansion(FactoryExpansionDefinition definition)
        {
            IReadOnlyList<FactoryExpansionDefinition> allExpansions = catalog != null ? catalog.AllExpansions : null;
            if (allExpansions == null || definition == null)
            {
                return -1;
            }

            for (int i = 0; i < allExpansions.Count; i++)
            {
                if (allExpansions[i] == definition)
                {
                    return i;
                }
            }

            return -1;
        }

        private FactoryExpansionDefinition ResolveExpansion(int index)
        {
            IReadOnlyList<FactoryExpansionDefinition> allExpansions = catalog != null ? catalog.AllExpansions : null;
            if (allExpansions == null || index < 0 || index >= allExpansions.Count)
            {
                return null;
            }

            return allExpansions[index];
        }

        private bool ExecuteExpansionPurchaseOnServer(int index)
        {
            FactoryExpansionDefinition definition = ResolveExpansion(index);
            bool success = definition != null && factoryExpansionManager.TryPurchase(definition);
            if (success && index < _syncedExpansionUnlocked.Count)
            {
                _syncedExpansionUnlocked[index] = true;
            }

            return success;
        }

        // ---- Power upgrade (Stage 11B extension) ----

        private void HandlePowerUpgradeChanged(bool previousValue, bool newValue)
        {
            if (IsServer || !_referencesValid)
            {
                return;
            }

            powerManager.RestoreUpgradeState(newValue);
        }

        private bool ExecutePowerUpgradeOnServer()
        {
            bool success = powerManager.TryPurchaseUpgrade();
            if (success)
            {
                _syncedPowerUpgradePurchased.Value = true;
            }

            return success;
        }

        // ---- Active order (Stage 11B extension) ----

        private void HandleActiveOrderIndexChanged(int previousValue, int newValue)
        {
            if (IsServer || !_referencesValid || newValue < 0)
            {
                // A transition back to -1 (completed, somewhere) is deliberately not
                // propagated into any client's OrderManager — there is nothing correct to
                // restore a cleared order TO; the next real accept will set a fresh index
                // (and fresh progress/state) anyway.
                return;
            }

            ApplyActiveOrderLocally(newValue);
        }

        /// <summary>(Stage 11C extension) Progress and state change independently of the
        /// index (packing one more item, or completing the batch, never touches which order
        /// is active) but both need the SAME apply call — re-reading all three synced values
        /// together, same as NetworkMachineController.ApplyLocally does for its own three
        /// fields.</summary>
        private void HandleActiveOrderProgressChanged(int previousValue, int newValue)
        {
            if (IsServer || !_referencesValid)
            {
                return;
            }

            ApplyActiveOrderLocally(_syncedActiveOrderIndex.Value);
        }

        private void HandleActiveOrderStateChanged(OrderState previousValue, OrderState newValue)
        {
            if (IsServer || !_referencesValid)
            {
                return;
            }

            ApplyActiveOrderLocally(_syncedActiveOrderIndex.Value);
        }

        private void ApplyActiveOrderLocally(int index)
        {
            OrderDefinition order = ResolveOrder(index);
            if (order == null)
            {
                return;
            }

            orderManager.RestoreOrderState(order, _syncedActiveOrderState.Value, _syncedActiveOrderProgress.Value, false);
        }

        /// <summary>(Stage 11C extension) Shared by the initial OnNetworkSpawn push and the
        /// per-frame Update() poll — reads the host's own OrderManager for whichever order is
        /// currently active (if any) and writes it into the two mirrors, same "only assign on
        /// an actual change" guard the balance/index mirrors already use.</summary>
        private void PushActiveOrderProgressAndState()
        {
            OrderDefinition active = orderManager.ActiveOrder;
            int progress = active != null ? orderManager.GetProgress(active) : 0;
            OrderState state = active != null ? orderManager.GetState(active) : OrderState.Available;

            if (_syncedActiveOrderProgress.Value != progress)
            {
                _syncedActiveOrderProgress.Value = progress;
            }

            if (_syncedActiveOrderState.Value != state)
            {
                _syncedActiveOrderState.Value = state;
            }
        }

        private int IndexOfActiveOrder()
        {
            OrderDefinition active = orderManager.ActiveOrder;
            return active != null ? IndexOfOrder(active) : -1;
        }

        private int IndexOfOrder(OrderDefinition order)
        {
            IReadOnlyList<OrderDefinition> allOrders = orderManager.AvailableOrders;
            if (allOrders == null || order == null)
            {
                return -1;
            }

            for (int i = 0; i < allOrders.Count; i++)
            {
                if (allOrders[i] == order)
                {
                    return i;
                }
            }

            return -1;
        }

        private OrderDefinition ResolveOrder(int index)
        {
            IReadOnlyList<OrderDefinition> allOrders = orderManager.AvailableOrders;
            if (allOrders == null || index < 0 || index >= allOrders.Count)
            {
                return null;
            }

            return allOrders[index];
        }

        private bool ExecuteOrderAcceptOnServer(int index)
        {
            OrderDefinition order = ResolveOrder(index);
            bool success = order != null && orderManager.TryAcceptOrder(order);
            if (success)
            {
                _syncedActiveOrderIndex.Value = index;
            }

            return success;
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

        // (Stage 11B bugfix) A client whose ServerRpc response never arrives (host-side
        // exception, dropped packet, host shutting down mid-request) used to wait forever —
        // MachinePlacementController/MachineShopPanelUI had no way to recover, which read as
        // "the game froze" after buying/placing. This is a second, independent safety net on
        // top of the try/catch below: even if the host silently drops the request, the
        // client un-sticks itself after a few seconds instead of waiting indefinitely.
        private const float RequestTimeoutSeconds = 6f;
        private float _purchaseRequestElapsed;
        private float _placementRequestElapsed;
        private float _expansionRequestElapsed;
        private float _powerUpgradeRequestElapsed;
        private float _orderAcceptRequestElapsed;

        private void Update()
        {
            if (!_referencesValid)
            {
                return;
            }

            // Polling rather than wrapping every EconomyManager call site: covers machine
            // purchases made here AND any other local spend/income this bridge doesn't know
            // about yet (F8 test money), so the shared balance always reflects whatever the
            // host's own authoritative EconomyManager currently holds — this also picks up
            // an order's reward the moment NetworkItemInteractionBridge.RequestDeliver credits
            // it host-side, with no extra wiring here. The active-order index/progress/state
            // are polled the same way so they clear/advance correctly regardless of whether
            // packing/delivery happened via this bridge's own request or the host's local E-press.
            if (IsServer)
            {
                if (economyManager.Balance != _syncedBalance.Value)
                {
                    _syncedBalance.Value = economyManager.Balance;
                }

                int currentActiveOrderIndex = IndexOfActiveOrder();
                if (currentActiveOrderIndex != _syncedActiveOrderIndex.Value)
                {
                    _syncedActiveOrderIndex.Value = currentActiveOrderIndex;
                }

                PushActiveOrderProgressAndState();

                return;
            }

            CheckRequestTimeout(ref _purchaseRequestElapsed, ref _pendingPurchaseCallback, "Satın alma");
            CheckRequestTimeout(ref _placementRequestElapsed, ref _pendingPlacementCallback, "Yerleştirme");
            CheckRequestTimeout(ref _expansionRequestElapsed, ref _pendingExpansionCallback, "Fabrika genişletme satın alma");
            CheckRequestTimeout(ref _powerUpgradeRequestElapsed, ref _pendingPowerUpgradeCallback, "Elektrik yükseltme");
            CheckRequestTimeout(ref _orderAcceptRequestElapsed, ref _pendingOrderAcceptCallback, "Sipariş kabul");
        }

        // (Stage 11B bugfix, generalized for the 11B extension) Same guaranteed-recovery
        // pattern as the purchase/placement timeout: if the host never replies (exception,
        // dropped packet, host shutting down mid-request), the client un-sticks itself
        // instead of leaving the requesting UI panel waiting forever.
        private static void CheckRequestTimeout(ref float elapsedSeconds, ref Action<bool, string> pendingCallback, string requestLabel)
        {
            if (pendingCallback == null)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds < RequestTimeoutSeconds)
            {
                return;
            }

            Debug.LogWarning($"[Shiftworks][MP] {requestLabel} isteğine host'tan yanıt gelmedi (zaman aşımı) — yerel olarak iptal ediliyor.");
            Action<bool, string> callback = pendingCallback;
            pendingCallback = null;
            callback.Invoke(false, "Host yanıt vermedi (bağlantı sorunu olabilir)");
        }

        /// <summary>Called by MachineShopPanelUI instead of MachinePurchaseManager.TryPurchase
        /// directly. On the host this is synchronous (same as single-player); on a remote
        /// client it fires a ServerRpc and calls onResult once the host replies.</summary>
        public void RequestPurchase(MachineDefinition definition, Action<bool, string> onResult)
        {
            if (!_referencesValid)
            {
                onResult?.Invoke(false, "Köprü yapılandırılmamış (bkz. Console hatası)");
                return;
            }

            int index = IndexOf(definition);
            if (index < 0)
            {
                Debug.LogWarning($"[Shiftworks][MP] Satın alma isteği reddedildi: '{definition}' SaveCatalog'da yok.");
                onResult?.Invoke(false, "Bilinmeyen makine (SaveCatalog'da yok)");
                return;
            }

            Debug.Log($"[Shiftworks][MP] Satın alma isteği: catalogIndex={index}, IsServer={IsServer}.");

            if (IsServer)
            {
                bool success = ExecutePurchaseOnServer(index);
                Debug.Log($"[Shiftworks][MP] Satın alma sonucu (host, yerel): {success}.");
                onResult?.Invoke(success, success ? "" : "Yetersiz bakiye!");
                return;
            }

            _pendingPurchaseCallback = onResult;
            _purchaseRequestElapsed = 0f;
            RequestPurchaseServerRpc(index);
        }

        /// <summary>Called by MachinePlacementController instead of
        /// MachinePurchaseManager.TryConsumePending + Instantiate directly.</summary>
        public void RequestPlacement(MachineDefinition definition, Vector3 position, float rotationY, Action<bool, string> onResult)
        {
            if (!_referencesValid)
            {
                onResult?.Invoke(false, "Köprü yapılandırılmamış (bkz. Console hatası)");
                return;
            }

            int index = IndexOf(definition);
            if (index < 0)
            {
                Debug.LogWarning($"[Shiftworks][MP] Yerleştirme isteği reddedildi: '{definition}' SaveCatalog'da yok.");
                onResult?.Invoke(false, "Bilinmeyen makine (SaveCatalog'da yok)");
                return;
            }

            Debug.Log($"[Shiftworks][MP] Yerleştirme isteği: catalogIndex={index}, position={position}, IsServer={IsServer}.");

            if (IsServer)
            {
                bool success = ExecutePlacementOnServer(index, position, rotationY);
                Debug.Log($"[Shiftworks][MP] Yerleştirme sonucu (host, yerel): {success}.");
                onResult?.Invoke(success, success ? "" : "Bekleyen makine yok (az önce başka biri yerleştirmiş olabilir)");
                return;
            }

            _pendingPlacementCallback = onResult;
            _placementRequestElapsed = 0f;
            RequestPlacementServerRpc(index, position, rotationY);
        }

        /// <summary>Called by FactoryExpansionPanelUI instead of
        /// FactoryExpansionManager.TryPurchase directly.</summary>
        public void RequestExpansionPurchase(FactoryExpansionDefinition definition, Action<bool, string> onResult)
        {
            if (!_referencesValid)
            {
                onResult?.Invoke(false, "Köprü yapılandırılmamış (bkz. Console hatası)");
                return;
            }

            int index = IndexOfExpansion(definition);
            if (index < 0)
            {
                Debug.LogWarning($"[Shiftworks][MP] Genişletme satın alma isteği reddedildi: '{definition}' SaveCatalog'da yok.");
                onResult?.Invoke(false, "Bilinmeyen genişletme (SaveCatalog'da yok)");
                return;
            }

            Debug.Log($"[Shiftworks][MP] Genişletme satın alma isteği: index={index}, IsServer={IsServer}.");

            if (IsServer)
            {
                bool success = ExecuteExpansionPurchaseOnServer(index);
                Debug.Log($"[Shiftworks][MP] Genişletme satın alma sonucu (host, yerel): {success}.");
                onResult?.Invoke(success, success ? "" : "Yetersiz bakiye veya zaten satın alınmış!");
                return;
            }

            _pendingExpansionCallback = onResult;
            _expansionRequestElapsed = 0f;
            RequestExpansionPurchaseServerRpc(index);
        }

        /// <summary>Called by PowerPanelUI instead of PowerManager.TryPurchaseUpgrade directly.</summary>
        public void RequestPowerUpgrade(Action<bool, string> onResult)
        {
            if (!_referencesValid)
            {
                onResult?.Invoke(false, "Köprü yapılandırılmamış (bkz. Console hatası)");
                return;
            }

            Debug.Log($"[Shiftworks][MP] Elektrik yükseltme isteği: IsServer={IsServer}.");

            if (IsServer)
            {
                bool success = ExecutePowerUpgradeOnServer();
                Debug.Log($"[Shiftworks][MP] Elektrik yükseltme sonucu (host, yerel): {success}.");
                onResult?.Invoke(success, success ? "" : "Yetersiz bakiye veya zaten satın alınmış!");
                return;
            }

            _pendingPowerUpgradeCallback = onResult;
            _powerUpgradeRequestElapsed = 0f;
            RequestPowerUpgradeServerRpc();
        }

        /// <summary>Called by OrderBoardPanelUI instead of OrderManager.TryAcceptOrder directly.</summary>
        public void RequestOrderAccept(OrderDefinition order, Action<bool, string> onResult)
        {
            if (!_referencesValid)
            {
                onResult?.Invoke(false, "Köprü yapılandırılmamış (bkz. Console hatası)");
                return;
            }

            int index = IndexOfOrder(order);
            if (index < 0)
            {
                Debug.LogWarning($"[Shiftworks][MP] Sipariş kabul isteği reddedildi: '{order}' OrderManager'da yok.");
                onResult?.Invoke(false, "Bilinmeyen sipariş");
                return;
            }

            Debug.Log($"[Shiftworks][MP] Sipariş kabul isteği: index={index}, IsServer={IsServer}.");

            if (IsServer)
            {
                bool success = ExecuteOrderAcceptOnServer(index);
                Debug.Log($"[Shiftworks][MP] Sipariş kabul sonucu (host, yerel): {success}.");
                onResult?.Invoke(success, success ? "" : "Sipariş kabul edilemedi — başka bir sipariş zaten aktif olabilir.");
                return;
            }

            _pendingOrderAcceptCallback = onResult;
            _orderAcceptRequestElapsed = 0f;
            RequestOrderAcceptServerRpc(index);
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

        // (Stage 11C bugfix) Matches MachinePlacementController's own ghost-overlap probe
        // exactly (same skin margin, same "Ignore Raycast"-only exclusion) — see that
        // class's own comment for why the skin shrink exists (so a machine resting flush on
        // the floor never counts the floor itself as a blocker). Re-running the SAME check
        // host-side (where the real, solid collider is about to appear) is the missing half:
        // the client's own copy of this check only ever validated the position against the
        // client's OWN local view of the world at THAT moment — a remote player's position
        // on the host can legitimately differ by a network tick or more (owner-authoritative
        // NetworkTransform), so a spot the client saw as clear could, by the time the host
        // actually processes the request, overlap a player on the host's own simulation.
        // Spawning a solid, non-trigger collider inside a live CharacterController makes
        // PhysX immediately depenetrate it — exactly the "character launched through the
        // floor" symptom reported after a client bought/placed a machine.
        private const float PlacementOverlapSkin = 0.03f;
        private static readonly Collider[] _placementOverlapBuffer = new Collider[8];

        private bool ExecutePlacementOnServer(int catalogIndex, Vector3 position, float rotationY)
        {
            MachineDefinition definition = ResolveDefinition(catalogIndex);
            if (definition == null || !purchaseManager.TryConsumePending(definition))
            {
                return false;
            }

            if (definition.MachinePrefab == null)
            {
                Debug.LogError($"[Shiftworks][MP] '{definition.MachineName}' için MachinePrefab atanmamış — yerleştirme iptal edildi, bekleyen sayısı geri eklendi.");
                purchaseManager.RestorePendingCount(definition, purchaseManager.GetPendingCount(definition) + 1);
                return false;
            }

            Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);
            if (IsPositionBlockedOnServer(definition.MachinePrefab, position, rotation, out string blockerName))
            {
                // TryConsumePending above already decremented the pending count — this is a
                // rejected placement, not a cancelled purchase, so it must be restored
                // exactly like the MachinePrefab-missing case above, or the player would
                // lose a machine they paid for without ever placing it.
                purchaseManager.RestorePendingCount(definition, purchaseManager.GetPendingCount(definition) + 1);
                Debug.LogWarning($"[Shiftworks][MP] Yerleştirme host tarafında reddedildi — '{blockerName}' ile çakışma " +
                    $"(konum: {position}). Bekleyen sayısı geri eklendi.");
                return false;
            }

            AdjustPendingCount(catalogIndex, -1);

            GameObject placed = Instantiate(definition.MachinePrefab, position, rotation);
            MachineController placedController = placed.GetComponent<MachineController>();
            if (placedController != null)
            {
                placedController.MarkAsPlayerPlaced(Guid.NewGuid().ToString("N"), definition);
            }

            NetworkObject placedNetworkObject = placed.GetComponent<NetworkObject>();
            if (placedNetworkObject != null)
            {
                placedNetworkObject.Spawn(true);
                Debug.Log($"[Shiftworks][MP] Makine ağa açıldı (NetworkObject.Spawn): '{definition.MachineName}' @ {position}.");
            }
            else
            {
                Debug.LogWarning($"[Shiftworks][MP] '{definition.MachineName}' prefab'ında NetworkObject yok — " +
                    "sadece host'ta görünecek, diğer istemcilere yayılmayacak.");
            }

            return true;
        }

        private static bool IsPositionBlockedOnServer(GameObject machinePrefab, Vector3 position, Quaternion rotation, out string blockerName)
        {
            blockerName = "";

            BoxCollider boxCollider = machinePrefab.GetComponent<BoxCollider>();
            Vector3 halfExtents = boxCollider != null
                ? Vector3.Scale(boxCollider.size, machinePrefab.transform.localScale) * 0.5f
                : machinePrefab.transform.localScale * 0.5f;

            Vector3 shrunkHalfExtents = new Vector3(
                Mathf.Max(0.01f, halfExtents.x - PlacementOverlapSkin),
                Mathf.Max(0.01f, halfExtents.y - PlacementOverlapSkin),
                Mathf.Max(0.01f, halfExtents.z - PlacementOverlapSkin));
            Vector3 overlapCenter = position + Vector3.up * PlacementOverlapSkin;

            int overlapCount = Physics.OverlapBoxNonAlloc(overlapCenter, shrunkHalfExtents, _placementOverlapBuffer,
                rotation, ~(1 << 2), QueryTriggerInteraction.Ignore);

            if (overlapCount > 0)
            {
                blockerName = _placementOverlapBuffer[0] != null ? _placementOverlapBuffer[0].name : "bilinmeyen";
                return true;
            }

            return false;
        }

        // (Stage 11B bugfix) Every server-side RPC handler is wrapped in try/catch and GUARANTEES
        // a ClientRpc reply even on failure — previously, any exception thrown while executing
        // the purchase/placement (e.g. a misconfigured prefab) would abort the RPC handler
        // silently: the requesting client's callback would never fire, leaving
        // MachinePlacementController/MachineShopPanelUI stuck waiting forever, which read as
        // "the client froze". The client-side timeout in Update() above is the second,
        // independent safety net for the same failure mode (e.g. a dropped reply packet).
        [ServerRpc(RequireOwnership = false)]
        private void RequestPurchaseServerRpc(int catalogIndex, ServerRpcParams rpcParams = default)
        {
            bool success;
            try
            {
                success = ExecutePurchaseOnServer(catalogIndex);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestPurchaseServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Satın alma sonucu (host, clientId={rpcParams.Receive.SenderClientId}): {success}.");
            NotifyPurchaseResultClientRpc(success, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPlacementServerRpc(int catalogIndex, Vector3 position, float rotationY, ServerRpcParams rpcParams = default)
        {
            // (Stage 11C bugfix) Explicitly requested diagnostic: logs the requester's own
            // host-side replicated position alongside the placement target, so if a
            // character launch is ever reported again the exact distance between "where the
            // host currently believes that player is" and "where the machine just appeared"
            // is immediately visible in the host's Console, without needing a second repro.
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            Vector3? requesterPosition = GetPlayerPositionOnServer(senderClientId);
            Debug.Log($"[Shiftworks][MP] Yerleştirme isteği: clientId={senderClientId}, hedef konum={position}, " +
                $"host'un bildiği oyuncu konumu={(requesterPosition.HasValue ? requesterPosition.Value.ToString() : "bulunamadı")}.");

            bool success;
            try
            {
                success = ExecutePlacementOnServer(catalogIndex, position, rotationY);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestPlacementServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Yerleştirme sonucu (host, clientId={senderClientId}): {success}.");
            NotifyPlacementResultClientRpc(success, TargetClient(senderClientId));
        }

        private Vector3? GetPlayerPositionOnServer(ulong clientId)
        {
            NetworkObject playerObject = NetworkManager != null ? NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId) : null;
            return playerObject != null ? playerObject.transform.position : (Vector3?)null;
        }

        [ClientRpc]
        private void NotifyPurchaseResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Debug.Log($"[Shiftworks][MP] Satın alma yanıtı alındı: {success}.");
            Action<bool, string> callback = _pendingPurchaseCallback;
            _pendingPurchaseCallback = null;
            callback?.Invoke(success, success ? "" : "Yetersiz bakiye! (host onaylamadı)");
        }

        [ClientRpc]
        private void NotifyPlacementResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Debug.Log($"[Shiftworks][MP] Yerleştirme yanıtı alındı: {success}.");
            Action<bool, string> callback = _pendingPlacementCallback;
            _pendingPlacementCallback = null;
            callback?.Invoke(success, success ? "" : "Bekleyen makine yok (az önce başka biri yerleştirmiş olabilir)");
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestExpansionPurchaseServerRpc(int index, ServerRpcParams rpcParams = default)
        {
            bool success;
            try
            {
                success = ExecuteExpansionPurchaseOnServer(index);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestExpansionPurchaseServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Genişletme satın alma sonucu (host, clientId={rpcParams.Receive.SenderClientId}): {success}.");
            NotifyExpansionPurchaseResultClientRpc(success, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPowerUpgradeServerRpc(ServerRpcParams rpcParams = default)
        {
            bool success;
            try
            {
                success = ExecutePowerUpgradeOnServer();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestPowerUpgradeServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Elektrik yükseltme sonucu (host, clientId={rpcParams.Receive.SenderClientId}): {success}.");
            NotifyPowerUpgradeResultClientRpc(success, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestOrderAcceptServerRpc(int index, ServerRpcParams rpcParams = default)
        {
            bool success;
            try
            {
                success = ExecuteOrderAcceptOnServer(index);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestOrderAcceptServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Sipariş kabul sonucu (host, clientId={rpcParams.Receive.SenderClientId}): {success}.");
            NotifyOrderAcceptResultClientRpc(success, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ClientRpc]
        private void NotifyExpansionPurchaseResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Debug.Log($"[Shiftworks][MP] Genişletme satın alma yanıtı alındı: {success}.");
            Action<bool, string> callback = _pendingExpansionCallback;
            _pendingExpansionCallback = null;
            callback?.Invoke(success, success ? "" : "Yetersiz bakiye veya zaten satın alınmış! (host onaylamadı)");
        }

        [ClientRpc]
        private void NotifyPowerUpgradeResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Debug.Log($"[Shiftworks][MP] Elektrik yükseltme yanıtı alındı: {success}.");
            Action<bool, string> callback = _pendingPowerUpgradeCallback;
            _pendingPowerUpgradeCallback = null;
            callback?.Invoke(success, success ? "" : "Yetersiz bakiye veya zaten satın alınmış! (host onaylamadı)");
        }

        [ClientRpc]
        private void NotifyOrderAcceptResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Debug.Log($"[Shiftworks][MP] Sipariş kabul yanıtı alındı: {success}.");
            Action<bool, string> callback = _pendingOrderAcceptCallback;
            _pendingOrderAcceptCallback = null;
            callback?.Invoke(success, success ? "" : "Sipariş kabul edilemedi — başka bir sipariş zaten aktif olabilir. (host onaylamadı)");
        }
    }
}
