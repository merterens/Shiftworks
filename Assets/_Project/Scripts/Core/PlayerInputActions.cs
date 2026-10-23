using UnityEngine;
using UnityEngine.InputSystem;

namespace Shiftworks.Core
{
    /// <summary>
    /// Hand-written wrapper around the new Input System's low-level InputAction API.
    /// Kept separate from FirstPersonController so that in a future multiplayer setup
    /// this can be created only for the locally-owned player instance.
    /// </summary>
    public class PlayerInputActions
    {
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool JumpPressedThisFrame { get; private set; }

        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _sprintAction;
        private readonly InputAction _jumpAction;

        public PlayerInputActions()
        {
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");

            _sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");

            _jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        }

        public void Enable()
        {
            _moveAction.Enable();
            _lookAction.Enable();
            _sprintAction.Enable();
            _jumpAction.Enable();
        }

        public void Disable()
        {
            _moveAction.Disable();
            _lookAction.Disable();
            _sprintAction.Disable();
            _jumpAction.Disable();
        }

        /// <summary>Call once per frame (e.g. from Update) before reading the properties above.</summary>
        public void Poll()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            LookInput = _lookAction.ReadValue<Vector2>();
            SprintHeld = _sprintAction.IsPressed();
            JumpPressedThisFrame = _jumpAction.WasPressedThisFrame();
        }

        public void Dispose()
        {
            _moveAction.Dispose();
            _lookAction.Dispose();
            _sprintAction.Dispose();
            _jumpAction.Dispose();
        }
    }
}
