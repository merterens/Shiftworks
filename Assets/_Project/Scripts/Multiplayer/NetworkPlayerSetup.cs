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
    /// Carry/interact/production systems are intentionally not part of this prototype.
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
        }
    }
}
