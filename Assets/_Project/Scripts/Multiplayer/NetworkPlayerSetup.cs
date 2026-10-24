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

        public override void OnNetworkSpawn()
        {
            bool isLocal = IsOwner;

            if (firstPersonController != null)
            {
                firstPersonController.IsLocalPlayer = isLocal;
                firstPersonController.enabled = isLocal;
            }

            if (playerCamera != null)
            {
                playerCamera.enabled = isLocal;
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
        }
    }
}
