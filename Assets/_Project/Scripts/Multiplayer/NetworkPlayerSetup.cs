using Shiftworks.Core.Interaction;
using Shiftworks.Items;
using Shiftworks.Player;
using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// Minimal ownership wiring for the multiplayer prototype's player. Toggles the exact
    /// same IsLocalPlayer switch FirstPersonController already exposed for this purpose back
    /// in Stage 1 ("set by future multiplayer/ownership code") — no changes to that script.
    ///
    /// FirstPersonController must start DISABLED on this prefab (see
    /// ShiftworksMultiplayerPrototypeSetup): every connected client instantiates a full
    /// NetworkPlayer instance for EVERY player including remote ones, so if the component
    /// started enabled, every remote representation's OnEnable() would also fire and lock
    /// the local cursor via Cursor.lockState (a global, not per-object, piece of state).
    /// Only enabling it here, for the owner only, keeps that side effect local to the one
    /// instance that should actually own input/cursor.
    ///
    /// Production/economy state itself is still not synced here (see NetworkEconomyBridge
    /// for Stage 11B's host-authoritative purchase/placement flow) — this class only ever
    /// toggles which local components may read local input/render for the owner.
    /// </summary>
    public class NetworkPlayerSetup : NetworkBehaviour
    {
        [SerializeField] private FirstPersonController firstPersonController;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;

        // (Stage 11 bugfix) Before this player's own camera is enabled below, the scene has
        // no active camera at all (nothing else in WorkshopMultiplayer.unity is a camera) —
        // Unity's "No cameras rendering" warning during that window is real, not cosmetic
        // noise. ShiftworksWorkshopMultiplayerSetup.AddLobbyCameraFallback() adds the scene's
        // one fallback camera that covers that window; found here by name since it's a scene
        // singleton, not something a prefab asset can reference directly.
        private const string LobbyCameraName = "LobbyCamera";

        public override void OnNetworkSpawn()
        {
            bool isLocal = IsOwner;
            Debug.Log($"[Shiftworks][MP] NetworkPlayer spawn edildi. OwnerClientId={OwnerClientId}, isLocal={isLocal}.");

            if (firstPersonController != null)
            {
                firstPersonController.IsLocalPlayer = isLocal;
                firstPersonController.enabled = isLocal;
            }

            if (playerCamera != null)
            {
                playerCamera.enabled = isLocal;
                if (isLocal)
                {
                    Debug.Log("[Shiftworks][MP] Yerel kamera etkinleştirildi.");
                }
            }

            if (audioListener != null)
            {
                audioListener.enabled = isLocal;
            }

            // (Stage 11B) Same reasoning as firstPersonController above: every client
            // instantiates one NetworkPlayer per connected player including remote ones, so
            // if these stayed enabled on a remote representation, its Update() would react
            // to this machine's own local keyboard/mouse (Keyboard.current is global, not
            // per-GameObject) exactly as if it were the local player — found via GetComponent
            // rather than new SerializeFields since both already live on this same prefab
            // root and are optional (absent on the plain MultiplayerPrototype player).
            PlayerInteractor interactor = GetComponent<PlayerInteractor>();
            if (interactor != null)
            {
                interactor.enabled = isLocal;
            }

            MachinePlacementController placement = GetComponent<MachinePlacementController>();
            if (placement != null)
            {
                placement.enabled = isLocal;
            }

            if (isLocal)
            {
                SetLobbyCameraActive(false);
                Debug.Log("[Shiftworks][MP] Oyuncu kontrolü etkinleştirildi (yerel oyuncu).");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                // Disconnected/scene-left while this was the local player — give the viewer
                // back a working camera instead of a blank "No cameras rendering" screen.
                SetLobbyCameraActive(true);
            }

            // (Stage 11C bugfix) Root cause of the repeating MissingReferenceException on
            // CurrentHolder.HoldPoint: CarryableItem.Interact/Drop only ever run on the host
            // now (11C-1's host-authoritative pickup), so a disconnecting/despawning
            // player's CurrentHolder reference on whatever item they were holding was never
            // cleared anywhere — the host's own FixedUpdate() kept chasing a HoldPoint
            // Transform that no longer existed, forever, and the item itself stayed stuck
            // "held" (IsHeld never false again) since nothing ever called Drop() on it.
            // Only the host's own simulation holds authoritative CurrentHolder references
            // (a client's own local copy of any item never has Interact() called on it at
            // all, so this would be a no-op there even without the IsServer guard).
            if (IsServer)
            {
                DropAnyItemHeldByThisPlayer();
            }

            Debug.Log($"[Shiftworks][MP] NetworkPlayer despawn edildi. OwnerClientId={OwnerClientId}.");
        }

        private void DropAnyItemHeldByThisPlayer()
        {
            PlayerInteractor interactor = GetComponent<PlayerInteractor>();
            if (interactor == null)
            {
                return;
            }

            CarryableItem[] allItems = FindObjectsByType<CarryableItem>(FindObjectsSortMode.None);
            foreach (CarryableItem item in allItems)
            {
                if (item.CurrentHolder == (IInteractor)interactor)
                {
                    item.Drop();
                    Debug.Log($"[Shiftworks][MP] '{item.name}', bağlantısı kesilen oyuncunun elinden " +
                        $"güvenli şekilde bırakıldı (OwnerClientId={OwnerClientId}).");
                }
            }
        }

        private static void SetLobbyCameraActive(bool active)
        {
            GameObject lobbyCameraGo = GameObject.Find(LobbyCameraName);
            if (lobbyCameraGo == null)
            {
                return;
            }

            Camera lobbyCamera = lobbyCameraGo.GetComponent<Camera>();
            if (lobbyCamera != null)
            {
                lobbyCamera.enabled = active;
            }

            AudioListener lobbyListener = lobbyCameraGo.GetComponent<AudioListener>();
            if (lobbyListener != null)
            {
                lobbyListener.enabled = active;
            }
        }
    }
}
