using Shiftworks.Items;
using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// (Stage 11C-1) One-way host -> everyone mirror of a single CarryableItem's
    /// held/automated state, so a non-host client's own "E - Al" prompt correctly disables
    /// for an item someone else (host or another client) already holds or an automated
    /// line already owns. CarryableItem itself is NOT a NetworkBehaviour (stays a plain
    /// MonoBehaviour so it keeps working completely unchanged in single-player, with zero
    /// NetworkObject/NetworkManager dependency) — this is a thin sibling component instead,
    /// same relationship NetworkEconomyBridge has to EconomyManager/MachinePurchaseManager.
    ///
    /// The item's actual Transform/physics sync is NetworkTransform + NetworkRigidbody's job
    /// (added alongside this on the same GameObject by the editor tool); this component only
    /// ever touches the one boolean CarryableItem.CanInteract needs, plus mirroring the same
    /// "Ignore Raycast" layer toggle CarryableItem.Interact/Drop already do locally (see
    /// ApplyUnavailable) — needed because that method only actually executes on the host now,
    /// so without this a client's own held item would stay on its normal layer on THEIR
    /// screen and block their own forward interaction raycast. Pickup/drop/throw REQUESTS
    /// are NetworkItemInteractionBridge's job, not this component's.
    /// </summary>
    public class NetworkCarryableItem : NetworkBehaviour
    {
        [SerializeField] private CarryableItem carryableItem;

        private readonly NetworkVariable<bool> _syncedUnavailable = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // (Stage 11C-1) CarryableItem.Interact/Drop also move the GameObject onto/off the
        // "Ignore Raycast" layer so the HOLDER's own forward-raycast (the held item sits
        // exactly on that axis) doesn't hit the item it is itself carrying — but that method
        // now only ever actually runs on the host. Without mirroring the layer here too, a
        // client's OWN held item would stay on its normal layer on that client's own screen
        // and block their own interaction raycast the moment they picked anything up.
        // Captured once at spawn so it is restored correctly regardless of which non-Default
        // layer a particular item prefab might use.
        private int _originalLayer;
        private bool _originalLayerCaptured;

        public override void OnNetworkSpawn()
        {
            if (carryableItem == null)
            {
                carryableItem = GetComponent<CarryableItem>();
            }

            if (!_originalLayerCaptured)
            {
                _originalLayer = gameObject.layer;
                _originalLayerCaptured = true;
            }

            _syncedUnavailable.OnValueChanged += HandleUnavailableChanged;

            if (IsServer)
            {
                _syncedUnavailable.Value = ComputeUnavailable();
            }
            else
            {
                ApplyUnavailable(_syncedUnavailable.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            _syncedUnavailable.OnValueChanged -= HandleUnavailableChanged;
        }

        private void Update()
        {
            if (!IsServer || carryableItem == null)
            {
                return;
            }

            bool unavailable = ComputeUnavailable();
            if (unavailable != _syncedUnavailable.Value)
            {
                _syncedUnavailable.Value = unavailable;
            }
        }

        private bool ComputeUnavailable() => carryableItem.IsHeld || carryableItem.IsAutomated;

        private void HandleUnavailableChanged(bool previousValue, bool newValue)
        {
            if (IsServer)
            {
                return;
            }

            ApplyUnavailable(newValue);
        }

        private void ApplyUnavailable(bool unavailable)
        {
            if (carryableItem != null)
            {
                carryableItem.SetNetworkAvailability(!unavailable);
            }

            gameObject.layer = unavailable ? LayerMask.NameToLayer("Ignore Raycast") : _originalLayer;
        }
    }
}
