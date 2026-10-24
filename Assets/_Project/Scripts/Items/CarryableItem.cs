using Shiftworks.Core.Interaction;
using UnityEngine;

namespace Shiftworks.Items
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CarryableItem : MonoBehaviour, IInteractable
    {
        [Header("Info")]
        [SerializeField] private ItemDefinition definition;
        // Fallback label used only when no ItemDefinition is assigned (keeps Stage 2's
        // plain test boxes working unchanged).
        [SerializeField] private string displayName = "Kutu";

        [Header("Carry")]
        // Safety ceiling only (not the primary mechanism): bounds how fast the item may
        // ever move while held, in case of a huge one-off gap (e.g. right after pickup).
        // The per-step velocity itself is computed from the real gap / Time.fixedDeltaTime
        // below, so normal sprint+look movement never has to hit this ceiling.
        [SerializeField] private float maxCatchUpSpeed = 40f;

        public IInteractor CurrentHolder { get; private set; }
        public bool IsHeld => CurrentHolder != null;
        public string InteractionPrompt => _cachedPrompt;
        public ItemDefinition Definition => definition;

        /// <summary>
        /// True while a feeder/conveyor/machine owns this item's movement (kinematic,
        /// moved by direct position assignment, not physics). Blocks manual pickup so the
        /// player and the automated line can never hold the same item at once.
        /// </summary>
        public bool IsAutomated { get; private set; }

        /// <summary>(Stage 11C-1) Defaults to true (no behavior change for single-player,
        /// where nothing ever calls SetNetworkAvailability). In multiplayer, NetworkCarryableItem
        /// mirrors whether the host-authoritative original of this item is already held/
        /// automated into this flag on every OTHER client, so their own raycast-driven
        /// CanInteract check (and therefore the "E - Al" prompt) correctly disables for an
        /// item someone else is already holding — without this, a client's local CurrentHolder
        /// would stay perpetually null (Interact() only ever actually runs on the host now)
        /// and the prompt would incorrectly stay enabled forever.</summary>
        private bool _networkAvailable = true;

        private Rigidbody _rigidbody;
        private Collider _collider;
        private string _cachedPrompt;
        private int _layerBeforeHeld;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            string label = definition != null ? definition.DisplayName : displayName;
            _cachedPrompt = $"E - Al ({label})";
        }

        public bool CanInteract(IInteractor interactor)
        {
            return !IsHeld && !IsAutomated && _networkAvailable;
        }

        /// <summary>(Stage 11C-1) Called only by NetworkCarryableItem, never by gameplay code —
        /// see the field's own doc comment.</summary>
        public void SetNetworkAvailability(bool available)
        {
            _networkAvailable = available;
        }

        public void Interact(IInteractor interactor)
        {
            if (IsHeld)
            {
                return;
            }

            if (IsAutomated)
            {
                EndAutomatedTransport();
            }

            CurrentHolder = interactor;

            // While held, the item sits exactly on the player's camera-forward axis (the
            // hold point), so it would otherwise be the first thing the interaction raycast
            // hits — blocking any attempt to look past it at a machine/station further away.
            // Moving it to the built-in "Ignore Raycast" layer excludes it from that raycast
            // specifically (PlayerInteractor masks this layer out) without touching physics
            // collision, which stays on the Default layer's normal collision matrix.
            _layerBeforeHeld = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            _rigidbody.useGravity = false;
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (interactor.BodyCollider != null)
            {
                Physics.IgnoreCollision(_collider, interactor.BodyCollider, true);
            }
        }

        public void Drop()
        {
            if (!IsHeld)
            {
                return;
            }

            if (CurrentHolder.BodyCollider != null)
            {
                Physics.IgnoreCollision(_collider, CurrentHolder.BodyCollider, false);
            }

            gameObject.layer = _layerBeforeHeld;
            _rigidbody.useGravity = true;
            _rigidbody.freezeRotation = false;
            CurrentHolder = null;
        }

        public void Throw(Vector3 velocity)
        {
            if (!IsHeld)
            {
                return;
            }

            Drop();
            _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        /// <summary>Called by a feeder/conveyor/machine taking ownership of this item.</summary>
        public void BeginAutomatedTransport(Vector3 worldPosition)
        {
            if (IsHeld)
            {
                return;
            }

            IsAutomated = true;
            _rigidbody.isKinematic = true;
            _rigidbody.detectCollisions = false;
            transform.position = worldPosition;
        }

        /// <summary>Called every step by whichever segment currently owns this item.</summary>
        public void SetAutomatedPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        private void EndAutomatedTransport()
        {
            IsAutomated = false;
            _rigidbody.isKinematic = false;
            _rigidbody.detectCollisions = true;
        }

        private void FixedUpdate()
        {
            if (!IsHeld)
            {
                return;
            }

            // (Stage 11C bugfix) Secondary safety net — the real fix is
            // NetworkPlayerSetup.OnNetworkDespawn now calling Drop() the moment a holder
            // disconnects, which is what actually clears CurrentHolder. This check exists
            // because IsHeld (CurrentHolder != null) can never detect that on its own:
            // CurrentHolder is typed as the IInteractor INTERFACE, so that comparison is
            // plain C# reference equality, not Unity's overridden "is the underlying
            // GameObject destroyed" check — a holder whose GameObject was destroyed through
            // some OTHER path this class doesn't know about would otherwise leave IsHeld
            // true forever and spam MissingReferenceException here every physics step.
            // HoldPoint is a real Transform reference, so this specific comparison DOES use
            // Unity's safe null check.
            if (CurrentHolder.HoldPoint == null)
            {
                Drop();
                return;
            }

            // Velocity needed to close the entire remaining gap within exactly one physics
            // step. This scales automatically with however far the hold point moved since
            // the last FixedUpdate (translation AND camera-turn arc), so the item never
            // structurally lags behind during a sprint + fast look-around, unlike a fixed
            // speed cap that was tuned only for straight-line sprint speed.
            Vector3 toTarget = CurrentHolder.HoldPoint.position - _rigidbody.position;
            Vector3 requiredVelocity = toTarget / Time.fixedDeltaTime;
            if (requiredVelocity.sqrMagnitude > maxCatchUpSpeed * maxCatchUpSpeed)
            {
                requiredVelocity = requiredVelocity.normalized * maxCatchUpSpeed;
            }

            _rigidbody.linearVelocity = requiredVelocity;
        }
    }
}
