using Shiftworks.Conveyors;
using Shiftworks.Core.Interaction;
using Shiftworks.Factory;
using Shiftworks.Items;
using Shiftworks.Machines;
using Shiftworks.Orders;
using Shiftworks.Power;
using Shiftworks.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shiftworks.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerInteractor : MonoBehaviour, IInteractor
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private InteractionPromptUI promptUI;

        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private float throwForce = 8f;

        [Header("Save/Load (Stage 10)")]
        [SerializeField] private SaveLoadMenuUI saveLoadMenuUI;

        public Transform HoldPoint => holdPoint;
        public Collider BodyCollider => _characterController;
        public CharacterController Controller => _characterController;

        /// <summary>(Stage 10) Read-only peek at what's currently held, purely for
        /// SaveManager to capture — mirrors the MachineController/ConveyorSegment pattern of
        /// a save-only read accessor that never lets outside code mutate the real field.</summary>
        public CarryableItem CarriedItem => _carriedItem;

        /// <summary>True while a modal UI panel (order board) owns input/cursor.</summary>
        public bool IsInputSuspended { get; private set; }

        // Excludes the built-in "Ignore Raycast" layer, which CarryableItem switches a
        // held item onto (see CarryableItem.Interact). Without this, the interaction
        // raycast always hits the player's own carried item first — it sits exactly on
        // the camera-forward axis 1.5m out — and can never reach anything beyond it.
        private static readonly int InteractionLayerMask = ~(1 << 2);

        private CharacterController _characterController;
        private FirstPersonController _firstPersonController;
        private MachinePlacementController _machinePlacementController;
        private IInteractable _currentTarget;
        private CarryableItem _carriedItem;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _firstPersonController = GetComponent<FirstPersonController>();
            _machinePlacementController = GetComponent<MachinePlacementController>();
        }

        private void Update()
        {
            if (IsInputSuspended)
            {
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                ClearTarget();
                return;
            }

            // Detection runs even while carrying an item: a machine needs to be lookable
            // (and its progress text refreshed) regardless of what the player is holding.
            UpdateLookTarget();

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                HandleInteractKey();
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleEscapeKey();
            }

            if (_carriedItem != null)
            {
                TryHandleDropOrThrow();
            }
        }

        /// <summary>
        /// (Stage 10) Only ever reached while nothing else is suspending input (the IsInputSuspended
        /// guard at the top of Update() already excludes every other open panel), so this can
        /// never fight an order/shop/power/expansion panel's own Escape-to-close handling —
        /// each of those still closes itself exactly as before. PlayerInteractor is now the
        /// ONLY thing that reacts to a bare Escape press (FirstPersonController no longer
        /// does) — two independent handlers racing on the same key was exactly the bug where
        /// pressing Escape sometimes only freed the cursor and never opened the menu,
        /// depending on which component's Update() Unity happened to run first that frame.
        /// </summary>
        private void HandleEscapeKey()
        {
            if (saveLoadMenuUI != null)
            {
                SuspendForUI();
                saveLoadMenuUI.Open(ResumeFromUI);
                return;
            }

            // No Save/Load menu wired yet (Stage 10's Editor menu hasn't been run) — fall
            // back to the simple "just free the mouse" behavior this key used to have.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>(Stage 10) Forces a freshly-spawned (never-held) item straight into the
        /// player's hand for a save restore — reuses CarryableItem.Interact's existing
        /// ownership/physics/layer setup (safe to call directly since a restored item is, by
        /// construction, never already held) instead of duplicating that logic here.</summary>
        public void RestoreCarriedItem(CarryableItem item)
        {
            if (item == null || _carriedItem != null)
            {
                return;
            }

            item.Interact(this);
            _carriedItem = item;
        }

        /// <summary>(Stage 10) Destroys whatever is currently held, for SaveManager's
        /// pre-load "clear dynamic state" pass — a load is about to respawn its own
        /// carried-item record (or none), so the previous session's held item (if any)
        /// must not linger. Distinct from RequestDrop: dropping leaves the item in the
        /// world, this removes it entirely, since it's about to be superseded by whatever
        /// the save file says (or by nothing).</summary>
        public void ClearCarriedItemForRestore()
        {
            if (_carriedItem == null)
            {
                return;
            }

            Destroy(_carriedItem.gameObject);
            _carriedItem = null;
        }

        private void UpdateLookTarget()
        {
            IInteractable found = null;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactionDistance, InteractionLayerMask))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable) && interactable.CanInteract(this))
                {
                    found = interactable;
                }
            }

            bool targetChanged = found != _currentTarget;
            _currentTarget = found;

            if (_currentTarget == null)
            {
                promptUI.Hide();
                return;
            }

            // These prompts change over time (progress %, stock, stored/packed count) even
            // while looking at the same object, so they must refresh every frame. Other
            // interactables have a static prompt while unchanged, so only refresh on an
            // actual target change to avoid redundant string work every frame.
            if (targetChanged || _currentTarget is MachineController or OutputStorage or RawMaterialFeeder
                or PackingStation or ShippingZone)
            {
                promptUI.Show(BuildPrompt(_currentTarget));
            }
        }

        private string BuildPrompt(IInteractable target)
        {
            switch (target)
            {
                case MachineController machine:
                    return machine.GetPromptFor(_carriedItem);
                case PackingStation station:
                    return station.GetPromptFor(_carriedItem);
                case ShippingZone zone:
                    return zone.GetPromptFor(_carriedItem);
                default:
                    return target.InteractionPrompt;
            }
        }

        private void ClearTarget()
        {
            if (_currentTarget == null)
            {
                return;
            }

            _currentTarget = null;
            promptUI.Hide();
        }

        private void HandleInteractKey()
        {
            if (_currentTarget == null)
            {
                return;
            }

            if (_currentTarget is MachineController machine)
            {
                HandleMachineInteraction(machine);
                return;
            }

            if (_currentTarget is OutputStorage storage)
            {
                HandleStorageInteraction(storage);
                return;
            }

            if (_currentTarget is OrderBoard board)
            {
                HandleOrderBoardInteraction(board);
                return;
            }

            if (_currentTarget is MachineShopTerminal terminal)
            {
                HandleShopTerminalInteraction(terminal);
                return;
            }

            if (_currentTarget is FactoryExpansionTerminal expansionTerminal)
            {
                HandleExpansionTerminalInteraction(expansionTerminal);
                return;
            }

            if (_currentTarget is PowerPanelTerminal powerTerminal)
            {
                HandlePowerPanelInteraction(powerTerminal);
                return;
            }

            if (_currentTarget is PackingStation station)
            {
                HandlePackingInteraction(station);
                return;
            }

            if (_currentTarget is ShippingZone zone)
            {
                HandleShippingInteraction(zone);
                return;
            }

            if (_carriedItem != null)
            {
                // Already carrying something; picking up a second loose item is not allowed.
                return;
            }

            RequestInteraction(_currentTarget);

            if (_currentTarget is CarryableItem item && item.CurrentHolder == (IInteractor)this)
            {
                _carriedItem = item;
            }
        }

        private void HandleMachineInteraction(MachineController machine)
        {
            // A machine with a finished part waiting can never accept new input (TryAcceptInput
            // requires Idle), so if the player's hands are full there is nothing a "feed"
            // attempt could do anyway — the only thing E can mean here is "deal with this
            // machine". Dropping first (exactly what G already does) and then collecting in
            // the same press removes the two-separate-keys dance (G, then E) that repeatedly
            // looked like a stuck machine: nothing is lost, the dropped item just lands at the
            // player's feet, pickup-able immediately after.
            if (machine.State == MachineState.OutputReady && _carriedItem != null)
            {
                RequestDrop(_carriedItem);
                _carriedItem = null;
            }

            if (_carriedItem != null)
            {
                if (machine.TryAcceptInput(_carriedItem))
                {
                    _carriedItem = null;
                }
                return;
            }

            if (machine.TryCollectOutput(this, out CarryableItem output))
            {
                _carriedItem = output;
            }
        }

        private void HandleStorageInteraction(OutputStorage storage)
        {
            if (_carriedItem != null)
            {
                // Already carrying something; withdrawing a second item is not allowed.
                return;
            }

            if (storage.TryWithdraw(this, out CarryableItem withdrawn))
            {
                _carriedItem = withdrawn;
            }
        }

        private void HandleOrderBoardInteraction(OrderBoard board)
        {
            SuspendForUI();
            board.OpenPanel(ResumeFromUI);
        }

        private void HandleShopTerminalInteraction(MachineShopTerminal terminal)
        {
            SuspendForUI();
            terminal.OpenPanel(ResumeFromUI, () =>
            {
                if (_machinePlacementController != null)
                {
                    _machinePlacementController.BeginPlacement(terminal.Definition, terminal.PurchaseManager);
                }
            });
        }

        private void HandleExpansionTerminalInteraction(FactoryExpansionTerminal terminal)
        {
            SuspendForUI();
            terminal.OpenPanel(ResumeFromUI);
        }

        private void HandlePowerPanelInteraction(PowerPanelTerminal terminal)
        {
            SuspendForUI();
            terminal.OpenPanel(ResumeFromUI);
        }

        private void HandlePackingInteraction(PackingStation station)
        {
            if (_carriedItem == null)
            {
                return;
            }

            if (station.TryPack(_carriedItem))
            {
                _carriedItem = null;
            }
        }

        private void HandleShippingInteraction(ShippingZone zone)
        {
            if (_carriedItem == null)
            {
                return;
            }

            if (zone.TryDeliver(_carriedItem))
            {
                _carriedItem = null;
            }
        }

        private void TryHandleDropOrThrow()
        {
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                RequestDrop(_carriedItem);
                _carriedItem = null;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                RequestThrow(_carriedItem, cameraTransform.forward * throwForce);
                _carriedItem = null;
            }
        }

        private void SuspendForUI()
        {
            IsInputSuspended = true;
            if (_firstPersonController != null)
            {
                _firstPersonController.IsInputSuspended = true;
            }

            ClearTarget();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ResumeFromUI()
        {
            IsInputSuspended = false;
            if (_firstPersonController != null)
            {
                _firstPersonController.IsInputSuspended = false;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Called by MachinePlacementController. Unlike SuspendForUI/ResumeFromUI, this never
        /// touches the cursor or FirstPersonController.IsInputSuspended — movement/look stay
        /// fully active so the player can walk around while placing. Only this script's own
        /// E/G/left-click handling is suspended, and FirstPersonController's Escape/click
        /// cursor-toggle is suppressed so it can't fight MachinePlacementController's own use
        /// of those same inputs (cancel / confirm).
        /// </summary>
        public void SetPlacementModeActive(bool active)
        {
            IsInputSuspended = active;
            if (_firstPersonController != null)
            {
                _firstPersonController.SuppressCursorToggle = active;
            }

            if (active)
            {
                ClearTarget();
            }
        }

        // Local-only for now. This indirection is the seam where a future host RPC call
        // (multiplayer interaction request) would be inserted without touching callers above.
        private void RequestInteraction(IInteractable target) => target.Interact(this);

        private void RequestDrop(CarryableItem item) => item.Drop();

        private void RequestThrow(CarryableItem item, Vector3 velocity) => item.Throw(velocity);
    }
}
