using System;
using Shiftworks.Conveyors;
using Shiftworks.Core.Interaction;
using Shiftworks.Items;
using Shiftworks.Machines;
using Shiftworks.Orders;
using Shiftworks.Player;
using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// (Stage 11C-1) Makes picking up a CarryableItem host-authoritative, the same
    /// request/validate/apply/reply shape as NetworkEconomyBridge — except there is no
    /// single manager to call through: CarryableItem.Interact/Drop/Throw already ARE the
    /// single atomic authority (same as EconomyManager.TrySpend), this only decides WHO may
    /// call them directly (the server) and resolves WHICH IInteractor a remote request
    /// came from.
    ///
    /// Pickup is request/confirm (can genuinely fail — someone else got there first — so the
    /// requester's own PlayerInteractor._carriedItem must not be set until the host
    /// confirms). Drop/Throw are fire-and-forget: CarryableItem.Drop/Throw cannot meaningfully
    /// fail once the caller is already holding the item, so the requester clears its own
    /// _carriedItem immediately for a responsive feel, exactly like single-player.
    ///
    /// A remote client has no local reference to its own PlayerInteractor as the host sees
    /// it; NetworkManager.SpawnManager.GetPlayerNetworkObject(senderClientId) resolves the
    /// host-side NetworkPlayer representing that specific client, whose PlayerInteractor is
    /// then passed into the exact same CarryableItem.Interact(IInteractor) every other path
    /// already uses — no changes to CarryableItem's own pickup logic were needed.
    ///
    /// Physical sync itself (position/rotation while held, thrown, or dropped) is
    /// NetworkTransform + NetworkRigidbody's job (added to every item alongside this bridge's
    /// sibling NetworkCarryableItem) — Server-authority mode, so the host's own physics
    /// simulation is the only one that ever actually runs; NetworkRigidbody automatically
    /// makes every non-authoritative Rigidbody kinematic, which is what prevents a client's
    /// own local physics from ever fighting the replicated position.
    ///
    /// (Stage 11C bugfix) RequestWithdraw extends the exact same shape to
    /// OutputStorage.TryWithdraw: StoredCount itself is mirrored separately by
    /// NetworkOutputStorage (a one-way NetworkVariable, same pattern as
    /// NetworkEconomyBridge's balance), while this bridge only gates WHO may call
    /// TryWithdraw directly and NetworkObject.Spawn's the item it produces — previously
    /// TryWithdraw ran unmodified on whichever client pressed E, so each client's own
    /// OutputStorage.StoredCount only ever decremented locally and the withdrawn item was a
    /// plain, never-spawned Instantiate() nobody else could ever see.
    /// </summary>
    public class NetworkItemInteractionBridge : NetworkBehaviour
    {
        public static NetworkItemInteractionBridge Instance { get; private set; }

        // (Stage 11C bugfix) Direct reference, not an index lookup like SaveCatalog-based
        // bridges use for machines/expansions — there is exactly one OutputStorage in the
        // scene today, so an index would be pure overhead.
        [SerializeField] private OutputStorage outputStorage;

        // (Stage 11C extension) Same reasoning — exactly one of each in the scene.
        [SerializeField] private PackingStation packingStation;
        [SerializeField] private ShippingZone shippingZone;

        // (Stage 11C diagnostic) Bumped whenever this file's feed-diagnostic logging changes,
        // so comparing Host Console vs Client Player.log immediately shows whether the
        // Client is running a build that even CONTAINS this diagnostic code — if this tag
        // never appears in the Client's log at all, the Client is running a stale build.
        private const string DiagnosticBuildTag = "11C-FeedDiag-1";

        private const float RequestTimeoutSeconds = 6f;
        private Action<bool, string> _pendingPickupCallback;
        private float _pickupRequestElapsed;
        private Action<bool, CarryableItem, string> _pendingWithdrawCallback;
        private float _withdrawRequestElapsed;
        private Action<bool, string> _pendingFeedCallback;
        private float _feedRequestElapsed;
        private Action<bool, CarryableItem, string> _pendingCollectCallback;
        private float _collectRequestElapsed;
        private int _feedRequestCounter;
        private Action<bool, string> _pendingPackCallback;
        private float _packRequestElapsed;
        private Action<bool, string> _pendingDeliverCallback;
        private float _deliverRequestElapsed;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            Debug.Log($"[Shiftworks][MP][BUILD] NetworkItemInteractionBridge.OnNetworkSpawn — " +
                $"tag={DiagnosticBuildTag}, IsServer={IsServer}, IsClient={IsClient}, " +
                $"LocalClientId={(NetworkManager != null ? NetworkManager.LocalClientId : 0UL)}.");
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (IsServer)
            {
                return;
            }

            if (_pendingPickupCallback != null)
            {
                _pickupRequestElapsed += Time.deltaTime;
                if (_pickupRequestElapsed >= RequestTimeoutSeconds)
                {
                    Debug.LogWarning("[Shiftworks][MP] Eşya alma isteğine host'tan yanıt gelmedi (zaman aşımı) — yerel olarak iptal ediliyor.");
                    Action<bool, string> callback = _pendingPickupCallback;
                    _pendingPickupCallback = null;
                    callback.Invoke(false, "Host yanıt vermedi (bağlantı sorunu olabilir)");
                }
            }

            if (_pendingWithdrawCallback != null)
            {
                _withdrawRequestElapsed += Time.deltaTime;
                if (_withdrawRequestElapsed >= RequestTimeoutSeconds)
                {
                    Debug.LogWarning("[Shiftworks][MP] Depo alma isteğine host'tan yanıt gelmedi (zaman aşımı) — yerel olarak iptal ediliyor.");
                    Action<bool, CarryableItem, string> callback = _pendingWithdrawCallback;
                    _pendingWithdrawCallback = null;
                    callback.Invoke(false, null, "Host yanıt vermedi (bağlantı sorunu olabilir)");
                }
            }

            if (_pendingFeedCallback != null)
            {
                _feedRequestElapsed += Time.deltaTime;
                if (_feedRequestElapsed >= RequestTimeoutSeconds)
                {
                    Debug.LogWarning("[Shiftworks][MP] Makine girdi isteğine host'tan yanıt gelmedi (zaman aşımı) — yerel olarak iptal ediliyor.");
                    Action<bool, string> callback = _pendingFeedCallback;
                    _pendingFeedCallback = null;
                    callback.Invoke(false, "Host yanıt vermedi (bağlantı sorunu olabilir)");
                }
            }

            if (_pendingCollectCallback != null)
            {
                _collectRequestElapsed += Time.deltaTime;
                if (_collectRequestElapsed >= RequestTimeoutSeconds)
                {
                    Debug.LogWarning("[Shiftworks][MP] Çıktı alma isteğine host'tan yanıt gelmedi (zaman aşımı) — yerel olarak iptal ediliyor.");
                    Action<bool, CarryableItem, string> callback = _pendingCollectCallback;
                    _pendingCollectCallback = null;
                    callback.Invoke(false, null, "Host yanıt vermedi (bağlantı sorunu olabilir)");
                }
            }

            if (_pendingPackCallback != null)
            {
                _packRequestElapsed += Time.deltaTime;
                if (_packRequestElapsed >= RequestTimeoutSeconds)
                {
                    Debug.LogWarning("[Shiftworks][MP] Paketleme isteğine host'tan yanıt gelmedi (zaman aşımı) — yerel olarak iptal ediliyor.");
                    Action<bool, string> callback = _pendingPackCallback;
                    _pendingPackCallback = null;
                    callback.Invoke(false, "Host yanıt vermedi (bağlantı sorunu olabilir)");
                }
            }

            if (_pendingDeliverCallback != null)
            {
                _deliverRequestElapsed += Time.deltaTime;
                if (_deliverRequestElapsed >= RequestTimeoutSeconds)
                {
                    Debug.LogWarning("[Shiftworks][MP] Teslimat isteğine host'tan yanıt gelmedi (zaman aşımı) — yerel olarak iptal ediliyor.");
                    Action<bool, string> callback = _pendingDeliverCallback;
                    _pendingDeliverCallback = null;
                    callback.Invoke(false, "Host yanıt vermedi (bağlantı sorunu olabilir)");
                }
            }
        }

        /// <summary>Called by PlayerInteractor instead of OutputStorage.TryWithdraw directly.
        /// On the host this is synchronous (same as single-player) except the withdrawn
        /// item is additionally NetworkObject.Spawn'd so it exists for every client, not
        /// just the host; on a remote client it fires a ServerRpc and resolves the actual
        /// spawned item from a NetworkObjectReference once the host replies.</summary>
        public void RequestWithdraw(IInteractor localInteractor, Action<bool, CarryableItem, string> onResult)
        {
            if (outputStorage == null)
            {
                onResult?.Invoke(false, null, "Depo köprüye bağlanmamış");
                return;
            }

            if (IsServer)
            {
                bool success = ExecuteWithdrawOnServer(localInteractor, out CarryableItem item);
                onResult?.Invoke(success, item, success ? "" : "Depo boş");
                return;
            }

            _pendingWithdrawCallback = onResult;
            _withdrawRequestElapsed = 0f;
            RequestWithdrawServerRpc();
        }

        private bool ExecuteWithdrawOnServer(IInteractor interactor, out CarryableItem item)
        {
            bool success = outputStorage.TryWithdraw(interactor, out item);
            if (success && item != null)
            {
                NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
                if (itemNetworkObject != null && !itemNetworkObject.IsSpawned)
                {
                    itemNetworkObject.Spawn(true);
                }
            }

            return success;
        }

        /// <summary>Called by PlayerInteractor instead of MachineController.TryAcceptInput
        /// directly. The item the player is carrying must already be networked (Stage 11C-1)
        /// for a remote request — the host independently verifies the requester is actually
        /// the item's CurrentHolder (its own replicated view, not the client's claim) before
        /// ever calling TryAcceptInput, so a desynced or spoofed request cannot consume an
        /// item it does not hold. Sync of the resulting State/BufferedInputCount is
        /// NetworkMachineController's job, not this method's — TryAcceptInput itself is
        /// completely unchanged.</summary>
        public void RequestFeedMachine(MachineController machine, CarryableItem carriedItem, IInteractor localInteractor, Action<bool, string> onResult)
        {
            NetworkObject machineNetworkObject = machine != null ? machine.GetComponent<NetworkObject>() : null;
            NetworkObject itemNetworkObject = carriedItem != null ? carriedItem.GetComponent<NetworkObject>() : null;
            int reqId = ++_feedRequestCounter;
            ulong localClientId = NetworkManager != null ? NetworkManager.LocalClientId : 0UL;

            // (Stage 11C diagnostic) The single most important line in this whole flow: it
            // says EXACTLY which of the two possible paths this specific feed attempt takes.
            // If this keeps saying "YEREL (FALLBACK)" for a machine/item that are supposedly
            // both already networked, the mismatch is in WHY IsSpawned reads false here, not
            // anywhere further down the RPC chain.
            Debug.Log($"[Shiftworks][MP][FEED#{reqId}] RequestFeedMachine çağrıldı — " +
                $"LocalClientId={localClientId}, IsServer={IsServer}, IsClient={IsClient}, " +
                $"makine={(machine != null ? machine.name : "null")}, " +
                $"makineNetObjId={(machineNetworkObject != null ? machineNetworkObject.NetworkObjectId.ToString() : "yok")}, " +
                $"makineIsSpawned={(machineNetworkObject != null ? machineNetworkObject.IsSpawned.ToString() : "yok")}, " +
                $"makinePos={(machine != null ? machine.transform.position.ToString() : "yok")}, " +
                $"makineState={(machine != null ? machine.State.ToString() : "yok")}, " +
                $"makineBufferedInput={(machine != null ? machine.BufferedInputCount.ToString() : "yok")}, " +
                $"ürün={(carriedItem != null ? carriedItem.Definition?.DisplayName : "null")}, " +
                $"ürünNetObjId={(itemNetworkObject != null ? itemNetworkObject.NetworkObjectId.ToString() : "yok")}, " +
                $"ürünIsSpawned={(itemNetworkObject != null ? itemNetworkObject.IsSpawned.ToString() : "yok")}.");

            if (machineNetworkObject == null || !machineNetworkObject.IsSpawned
                || itemNetworkObject == null || !itemNetworkObject.IsSpawned)
            {
                // Scene-baked original machine (predates Stage 11B's NetworkObject addition
                // to the purchasable prefabs) or a non-networked item — fall back to the
                // exact original single-player call. This branch running for a PURCHASED,
                // supposedly-networked machine/item is itself the bug — logged loudly so it
                // cannot be missed in the Console/Player.log.
                bool localSuccess = machine != null && machine.TryAcceptInput(carriedItem);
                Debug.LogWarning($"[Shiftworks][MP][FEED#{reqId}] YEREL (FALLBACK) yola düşüldü — " +
                    $"istek HOST'A HİÇ GİTMEDİ. localSuccess={localSuccess}, " +
                    $"işlem sonrası yerel BufferedInputCount={(machine != null ? machine.BufferedInputCount.ToString() : "yok")}.");
                onResult?.Invoke(localSuccess, localSuccess ? "" : "Girdi kabul edilemedi");
                return;
            }

            Debug.Log($"[Shiftworks][MP][FEED#{reqId}] CLIENT_REQUEST_SENT (veya host-yerel çağrı) — " +
                $"oyuncu={DescribeInteractor(localInteractor)}, makine={machine.name}, " +
                $"taşınan={carriedItem.Definition?.DisplayName}, mevcut girdi={machine.BufferedInputCount}, " +
                $"durum={machine.State}, IsServer={IsServer}.");

            if (IsServer)
            {
                bool success = ExecuteFeedOnServer(machine, carriedItem, localInteractor, reqId);
                Debug.Log($"[Shiftworks][MP][FEED#{reqId}] Makine girdi sonucu (host, yerel): {success}.");
                onResult?.Invoke(success, success ? "" : "Girdi kabul edilemedi");
                return;
            }

            _pendingFeedCallback = onResult;
            _feedRequestElapsed = 0f;
            RequestFeedServerRpc(new NetworkObjectReference(machineNetworkObject), new NetworkObjectReference(itemNetworkObject), reqId);
        }

        /// <summary>Called by PlayerInteractor instead of MachineController.TryCollectOutput
        /// directly. Collection can genuinely fail (someone else collected it first), and the
        /// collected item must be resolved from a NetworkObjectReference on a remote request
        /// (same shape as RequestWithdraw) since it was already NetworkObject.Spawn'd by
        /// NetworkMachineController the moment the host's own machine completed its cycle.</summary>
        public void RequestCollectOutput(MachineController machine, IInteractor localInteractor, Action<bool, CarryableItem, string> onResult)
        {
            NetworkObject machineNetworkObject = machine != null ? machine.GetComponent<NetworkObject>() : null;
            if (machineNetworkObject == null || !machineNetworkObject.IsSpawned)
            {
                // (Compile fix) localItem must be assigned on every path reaching the
                // Invoke below — short-circuiting the "machine != null &&" check would
                // otherwise skip the TryCollectOutput call entirely and leave an out
                // variable declared inline unassigned (CS0165), so it is pre-declared here.
                CarryableItem localItem = null;
                bool localSuccess = machine != null && machine.TryCollectOutput(localInteractor, out localItem);
                onResult?.Invoke(localSuccess, localItem, localSuccess ? "" : "Çıktı hazır değil");
                return;
            }

            if (IsServer)
            {
                bool success = ExecuteCollectOnServer(machine, localInteractor, out CarryableItem item);
                onResult?.Invoke(success, item, success ? "" : "Çıktı hazır değil");
                return;
            }

            _pendingCollectCallback = onResult;
            _collectRequestElapsed = 0f;
            RequestCollectServerRpc(new NetworkObjectReference(machineNetworkObject));
        }

        private static bool ExecuteFeedOnServer(MachineController machine, CarryableItem item, IInteractor interactor, int reqId)
        {
            if (machine == null || item == null || interactor == null)
            {
                Debug.LogWarning($"[Shiftworks][MP][FEED#{reqId}] SERVER_REQUEST_REJECTED — " +
                    $"null referans (makine={machine != null}, ürün={item != null}, interactor={interactor != null}).");
                return false;
            }

            // The user's own explicit anti-spoofing requirement: verify the host's OWN view
            // of who is holding this item matches the resolved requester before ever
            // touching the machine, rather than trusting the client's claim.
            if (item.CurrentHolder != interactor)
            {
                Debug.LogWarning($"[Shiftworks][MP][FEED#{reqId}] SERVER_REQUEST_REJECTED — host'un gördüğü " +
                    $"taşıyan ({DescribeInteractor(item.CurrentHolder)}), isteği gönderen oyuncuyla " +
                    $"({DescribeInteractor(interactor)}) eşleşmiyor.");
                return false;
            }

            int bufferedBefore = machine.BufferedInputCount;
            bool accepted = machine.TryAcceptInput(item);
            if (!accepted)
            {
                Debug.LogWarning($"[Shiftworks][MP][FEED#{reqId}] SERVER_REQUEST_REJECTED — " +
                    $"TryAcceptInput false döndü (muhtemelen State != Idle, ürün tarifle uyuşmuyor, " +
                    $"veya güç yetersiz). Durum={machine.State}, BufferedInputCount={bufferedBefore}.");
            }
            else
            {
                Debug.Log($"[Shiftworks][MP][FEED#{reqId}] SERVER_REQUEST_ACCEPTED — " +
                    $"TryAcceptInput true. SERVER_INPUT_COUNT_UPDATED: {bufferedBefore} -> {machine.BufferedInputCount}, " +
                    $"yeni durum={machine.State}, makineNetObjId={machine.GetComponent<NetworkObject>()?.NetworkObjectId}.");
            }

            return accepted;
        }

        private static bool ExecuteCollectOnServer(MachineController machine, IInteractor interactor, out CarryableItem item)
        {
            bool success = machine.TryCollectOutput(interactor, out item);
            if (success && item != null)
            {
                NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
                if (itemNetworkObject != null && !itemNetworkObject.IsSpawned)
                {
                    // Should already have been spawned by NetworkMachineController the
                    // moment the cycle completed — this is a defensive fallback only.
                    itemNetworkObject.Spawn(true);
                }
            }

            return success;
        }

        private static string DescribeInteractor(IInteractor interactor) => interactor?.ToString() ?? "bilinmeyen";

        /// <summary>Called by PlayerInteractor instead of CarryableItem.Interact directly.
        /// localInteractor is always the CALLER's own local PlayerInteractor — on the host
        /// path it is used as-is; on the client path the host resolves the real requester
        /// itself from the RPC's sender id, so localInteractor is never sent over the
        /// network.</summary>
        public void RequestPickup(CarryableItem item, IInteractor localInteractor, Action<bool, string> onResult)
        {
            NetworkObject itemNetworkObject = item != null ? item.GetComponent<NetworkObject>() : null;
            if (itemNetworkObject == null || !itemNetworkObject.IsSpawned)
            {
                // Not a networked item yet (e.g. a machine/feeder-produced item before Stage
                // 11C-2/3 makes production itself networked) — fall back to the exact
                // original single-player call so pickup still works locally.
                item?.Interact(localInteractor);
                bool localSuccess = item != null && item.CurrentHolder == localInteractor;
                onResult?.Invoke(localSuccess, localSuccess ? "" : "Ürün alınamadı");
                return;
            }

            if (IsServer)
            {
                bool success = ExecutePickupOnServer(item, localInteractor);
                onResult?.Invoke(success, success ? "" : "Ürün zaten elde veya kullanımda");
                return;
            }

            _pendingPickupCallback = onResult;
            _pickupRequestElapsed = 0f;
            RequestPickupServerRpc(new NetworkObjectReference(itemNetworkObject));
        }

        /// <summary>Called by PlayerInteractor instead of CarryableItem.Drop directly.
        /// Fire-and-forget — see class doc comment for why.</summary>
        public void RequestDrop(CarryableItem item)
        {
            NetworkObject itemNetworkObject = item != null ? item.GetComponent<NetworkObject>() : null;
            if (itemNetworkObject == null || !itemNetworkObject.IsSpawned)
            {
                item?.Drop();
                return;
            }

            if (IsServer)
            {
                item.Drop();
                return;
            }

            RequestDropServerRpc(new NetworkObjectReference(itemNetworkObject));
        }

        /// <summary>Called by PlayerInteractor instead of CarryableItem.Throw directly.
        /// Fire-and-forget — see class doc comment for why.</summary>
        public void RequestThrow(CarryableItem item, Vector3 velocity)
        {
            NetworkObject itemNetworkObject = item != null ? item.GetComponent<NetworkObject>() : null;
            if (itemNetworkObject == null || !itemNetworkObject.IsSpawned)
            {
                item?.Throw(velocity);
                return;
            }

            if (IsServer)
            {
                item.Throw(velocity);
                return;
            }

            RequestThrowServerRpc(new NetworkObjectReference(itemNetworkObject), velocity);
        }

        private static bool ExecutePickupOnServer(CarryableItem item, IInteractor interactor)
        {
            if (item == null || interactor == null || !item.CanInteract(interactor))
            {
                return false;
            }

            item.Interact(interactor);
            return item.CurrentHolder == interactor;
        }

        private PlayerInteractor ResolveRequester(ulong clientId)
        {
            NetworkObject playerObject = NetworkManager != null ? NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId) : null;
            return playerObject != null ? playerObject.GetComponent<PlayerInteractor>() : null;
        }

        private static ClientRpcParams TargetClient(ulong clientId) => new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };

        [ServerRpc(RequireOwnership = false)]
        private void RequestPickupServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
        {
            bool success = false;
            try
            {
                if (itemRef.TryGet(out NetworkObject itemNetworkObject))
                {
                    CarryableItem item = itemNetworkObject.GetComponent<CarryableItem>();
                    PlayerInteractor requester = ResolveRequester(rpcParams.Receive.SenderClientId);
                    success = ExecutePickupOnServer(item, requester);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestPickupServerRpc İSTİSNA: {exception}");
                success = false;
            }

            NotifyPickupResultClientRpc(success, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDropServerRpc(NetworkObjectReference itemRef)
        {
            try
            {
                if (itemRef.TryGet(out NetworkObject itemNetworkObject))
                {
                    itemNetworkObject.GetComponent<CarryableItem>()?.Drop();
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestDropServerRpc İSTİSNA: {exception}");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestThrowServerRpc(NetworkObjectReference itemRef, Vector3 velocity)
        {
            try
            {
                if (itemRef.TryGet(out NetworkObject itemNetworkObject))
                {
                    itemNetworkObject.GetComponent<CarryableItem>()?.Throw(velocity);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestThrowServerRpc İSTİSNA: {exception}");
            }
        }

        [ClientRpc]
        private void NotifyPickupResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Action<bool, string> callback = _pendingPickupCallback;
            _pendingPickupCallback = null;
            callback?.Invoke(success, success ? "" : "Ürün zaten elde veya kullanımda (host onaylamadı)");
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestWithdrawServerRpc(ServerRpcParams rpcParams = default)
        {
            bool success = false;
            NetworkObjectReference itemRef = default;
            try
            {
                PlayerInteractor requester = ResolveRequester(rpcParams.Receive.SenderClientId);
                success = ExecuteWithdrawOnServer(requester, out CarryableItem item);
                if (success && item != null)
                {
                    NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
                    if (itemNetworkObject != null && itemNetworkObject.IsSpawned)
                    {
                        itemRef = new NetworkObjectReference(itemNetworkObject);
                    }
                    else
                    {
                        // The item exists on the host but has no NetworkObject (or failed to
                        // spawn) — the client would never be able to resolve it, so treat
                        // this the same as a failed withdrawal rather than silently handing
                        // the host a "held" item nobody else can ever see.
                        success = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestWithdrawServerRpc İSTİSNA: {exception}");
                success = false;
            }

            NotifyWithdrawResultClientRpc(success, itemRef, TargetClient(rpcParams.Receive.SenderClientId));
        }

        [ClientRpc]
        private void NotifyWithdrawResultClientRpc(bool success, NetworkObjectReference itemRef, ClientRpcParams rpcParams = default)
        {
            CarryableItem item = null;
            if (success && itemRef.TryGet(out NetworkObject itemNetworkObject))
            {
                item = itemNetworkObject.GetComponent<CarryableItem>();
            }

            Action<bool, CarryableItem, string> callback = _pendingWithdrawCallback;
            _pendingWithdrawCallback = null;
            callback?.Invoke(success && item != null, item, success ? "" : "Depo boş veya alınamadı (host onaylamadı)");
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestFeedServerRpc(NetworkObjectReference machineRef, NetworkObjectReference itemRef, int reqId, ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            bool success = false;
            Debug.Log($"[Shiftworks][MP][FEED#{reqId}] SERVER_REQUEST_RECEIVED — clientId={senderClientId}, " +
                $"IsServer={IsServer}, IsClient={IsClient}, LocalClientId={NetworkManager.LocalClientId}.");
            try
            {
                PlayerInteractor requester = ResolveRequester(senderClientId);
                if (machineRef.TryGet(out NetworkObject machineNetworkObject) && itemRef.TryGet(out NetworkObject itemNetworkObject))
                {
                    MachineController machine = machineNetworkObject.GetComponent<MachineController>();
                    CarryableItem item = itemNetworkObject.GetComponent<CarryableItem>();
                    Debug.Log($"[Shiftworks][MP][FEED#{reqId}] Host çözümledi — " +
                        $"makine={machine?.name}, makineNetObjId={machineNetworkObject.NetworkObjectId}, " +
                        $"makineIsSpawned={machineNetworkObject.IsSpawned}, makinePos={machine?.transform.position}, " +
                        $"taşınan={item?.Definition?.DisplayName}, ürünNetObjId={itemNetworkObject.NetworkObjectId}, " +
                        $"mevcut girdi={machine?.BufferedInputCount}, durum={machine?.State}, " +
                        $"requester={DescribeInteractor(requester)}.");
                    success = ExecuteFeedOnServer(machine, item, requester, reqId);
                }
                else
                {
                    Debug.LogWarning($"[Shiftworks][MP][FEED#{reqId}] SERVER_REQUEST_REJECTED — clientId={senderClientId}, " +
                        $"NetworkObjectReference çözümlenemedi (makine bulundu={machineRef.TryGet(out _)}, " +
                        $"ürün bulundu={itemRef.TryGet(out _)}) — host bu NetworkObjectId'leri henüz tanımıyor olabilir.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP][FEED#{reqId}] RequestFeedServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP][FEED#{reqId}] Makine girdi sonucu (host, clientId={senderClientId}): {(success ? "KABUL EDİLDİ" : "REDDEDİLDİ")}.");
            NotifyFeedResultClientRpc(success, reqId, TargetClient(senderClientId));
        }

        [ClientRpc]
        private void NotifyFeedResultClientRpc(bool success, int reqId, ClientRpcParams rpcParams = default)
        {
            Debug.Log($"[Shiftworks][MP][FEED#{reqId}] CLIENT_STATE_RECEIVED (RPC yanıtı) — success={success}, " +
                $"LocalClientId={NetworkManager.LocalClientId}.");
            Action<bool, string> callback = _pendingFeedCallback;
            _pendingFeedCallback = null;
            callback?.Invoke(success, success ? "" : "Girdi kabul edilemedi (host onaylamadı)");
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestCollectServerRpc(NetworkObjectReference machineRef, ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            bool success = false;
            NetworkObjectReference itemRef = default;
            try
            {
                PlayerInteractor requester = ResolveRequester(senderClientId);
                if (machineRef.TryGet(out NetworkObject machineNetworkObject))
                {
                    MachineController machine = machineNetworkObject.GetComponent<MachineController>();
                    success = ExecuteCollectOnServer(machine, requester, out CarryableItem item);
                    if (success && item != null)
                    {
                        NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
                        if (itemNetworkObject != null && itemNetworkObject.IsSpawned)
                        {
                            itemRef = new NetworkObjectReference(itemNetworkObject);
                        }
                        else
                        {
                            success = false;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestCollectServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Çıktı alma sonucu (host, clientId={senderClientId}): {success}.");
            NotifyCollectResultClientRpc(success, itemRef, TargetClient(senderClientId));
        }

        [ClientRpc]
        private void NotifyCollectResultClientRpc(bool success, NetworkObjectReference itemRef, ClientRpcParams rpcParams = default)
        {
            CarryableItem item = null;
            if (success && itemRef.TryGet(out NetworkObject itemNetworkObject))
            {
                item = itemNetworkObject.GetComponent<CarryableItem>();
            }

            Action<bool, CarryableItem, string> callback = _pendingCollectCallback;
            _pendingCollectCallback = null;
            callback?.Invoke(success && item != null, item, success ? "" : "Çıktı hazır değil (host onaylamadı)");
        }

        /// <summary>(Stage 11C extension) Called by PlayerInteractor instead of
        /// PackingStation.TryPack directly — same host-authoritative shape as
        /// RequestFeedMachine (anti-spoof CurrentHolder check, consumption only ever runs on
        /// the host). PackingStation.TryRegisterPackedItem's own progress count is unchanged;
        /// NetworkEconomyBridge separately mirrors it for display (see its own class doc
        /// comment). If packing this item completed the order's batch, the resulting
        /// ShipmentPackage (already Instantiate'd by TryPack, exposed via
        /// PackingStation.LastSpawnedPackage) is NetworkObject.Spawn'd here so every client
        /// can see and pick it up through the existing RequestPickup path — no new pickup
        /// logic needed.</summary>
        public void RequestPack(CarryableItem carriedItem, IInteractor localInteractor, Action<bool, string> onResult)
        {
            if (packingStation == null)
            {
                onResult?.Invoke(false, "Paketleme istasyonu köprüye bağlanmamış");
                return;
            }

            NetworkObject itemNetworkObject = carriedItem != null ? carriedItem.GetComponent<NetworkObject>() : null;
            if (itemNetworkObject == null || !itemNetworkObject.IsSpawned)
            {
                bool localSuccess = packingStation.TryPack(carriedItem);
                onResult?.Invoke(localSuccess, localSuccess ? "" : "Paketlenemedi");
                return;
            }

            if (IsServer)
            {
                bool success = ExecutePackOnServer(carriedItem, localInteractor);
                onResult?.Invoke(success, success ? "" : "Paketlenemedi");
                return;
            }

            _pendingPackCallback = onResult;
            _packRequestElapsed = 0f;
            RequestPackServerRpc(new NetworkObjectReference(itemNetworkObject));
        }

        private bool ExecutePackOnServer(CarryableItem item, IInteractor interactor)
        {
            if (item == null || interactor == null || packingStation == null)
            {
                return false;
            }

            if (item.CurrentHolder != interactor)
            {
                Debug.LogWarning("[Shiftworks][MP] Paketleme isteği reddedildi — host'un gördüğü taşıyan, " +
                    "isteği gönderen oyuncuyla eşleşmiyor.");
                return false;
            }

            bool success = packingStation.TryPack(item);
            if (success)
            {
                SpawnPackageIfNeeded(packingStation.LastSpawnedPackage);
            }

            return success;
        }

        private static void SpawnPackageIfNeeded(ShipmentPackage package)
        {
            if (package == null)
            {
                return;
            }

            NetworkObject packageNetworkObject = package.GetComponent<NetworkObject>();
            if (packageNetworkObject != null && !packageNetworkObject.IsSpawned)
            {
                packageNetworkObject.Spawn(true);
                Debug.Log($"[Shiftworks][MP] Sevkiyat paketi ağa açıldı (NetworkObject.Spawn): '{package.name}'.");
            }
        }

        /// <summary>(Stage 11C extension) Called by PlayerInteractor instead of
        /// ShippingZone.TryDeliver directly — same shape as RequestPack. TryCompleteOrder's
        /// reward credit lands in EconomyManager.Balance, already mirrored by
        /// NetworkEconomyBridge's existing balance sync; the order's own ReadyToShip ->
        /// Completed transition is likewise already covered by that bridge's active-order
        /// index/state mirror (the index goes to -1 the next time it polls), so this method
        /// does not need to push anything beyond consuming the package.</summary>
        public void RequestDeliver(CarryableItem carriedItem, IInteractor localInteractor, Action<bool, string> onResult)
        {
            if (shippingZone == null)
            {
                onResult?.Invoke(false, "Sevkiyat alanı köprüye bağlanmamış");
                return;
            }

            NetworkObject itemNetworkObject = carriedItem != null ? carriedItem.GetComponent<NetworkObject>() : null;
            if (itemNetworkObject == null || !itemNetworkObject.IsSpawned)
            {
                bool localSuccess = shippingZone.TryDeliver(carriedItem);
                onResult?.Invoke(localSuccess, localSuccess ? "" : "Teslim edilemedi");
                return;
            }

            if (IsServer)
            {
                bool success = ExecuteDeliverOnServer(carriedItem, localInteractor);
                onResult?.Invoke(success, success ? "" : "Teslim edilemedi");
                return;
            }

            _pendingDeliverCallback = onResult;
            _deliverRequestElapsed = 0f;
            RequestDeliverServerRpc(new NetworkObjectReference(itemNetworkObject));
        }

        private bool ExecuteDeliverOnServer(CarryableItem item, IInteractor interactor)
        {
            if (item == null || interactor == null || shippingZone == null)
            {
                return false;
            }

            if (item.CurrentHolder != interactor)
            {
                Debug.LogWarning("[Shiftworks][MP] Teslimat isteği reddedildi — host'un gördüğü taşıyan, " +
                    "isteği gönderen oyuncuyla eşleşmiyor.");
                return false;
            }

            return shippingZone.TryDeliver(item);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPackServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            bool success = false;
            try
            {
                PlayerInteractor requester = ResolveRequester(senderClientId);
                if (itemRef.TryGet(out NetworkObject itemNetworkObject))
                {
                    CarryableItem item = itemNetworkObject.GetComponent<CarryableItem>();
                    success = ExecutePackOnServer(item, requester);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestPackServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Paketleme sonucu (host, clientId={senderClientId}): {(success ? "KABUL EDİLDİ" : "REDDEDİLDİ")}.");
            NotifyPackResultClientRpc(success, TargetClient(senderClientId));
        }

        [ClientRpc]
        private void NotifyPackResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Action<bool, string> callback = _pendingPackCallback;
            _pendingPackCallback = null;
            callback?.Invoke(success, success ? "" : "Paketlenemedi (host onaylamadı)");
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDeliverServerRpc(NetworkObjectReference itemRef, ServerRpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            bool success = false;
            try
            {
                PlayerInteractor requester = ResolveRequester(senderClientId);
                if (itemRef.TryGet(out NetworkObject itemNetworkObject))
                {
                    CarryableItem item = itemNetworkObject.GetComponent<CarryableItem>();
                    success = ExecuteDeliverOnServer(item, requester);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Shiftworks][MP] RequestDeliverServerRpc İSTİSNA: {exception}");
                success = false;
            }

            Debug.Log($"[Shiftworks][MP] Teslimat sonucu (host, clientId={senderClientId}): {(success ? "KABUL EDİLDİ" : "REDDEDİLDİ")}.");
            NotifyDeliverResultClientRpc(success, TargetClient(senderClientId));
        }

        [ClientRpc]
        private void NotifyDeliverResultClientRpc(bool success, ClientRpcParams rpcParams = default)
        {
            Action<bool, string> callback = _pendingDeliverCallback;
            _pendingDeliverCallback = null;
            callback?.Invoke(success, success ? "" : "Teslim edilemedi (host onaylamadı)");
        }
    }
}
