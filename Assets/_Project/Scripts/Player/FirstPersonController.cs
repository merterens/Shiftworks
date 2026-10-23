using Shiftworks.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shiftworks.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -18f;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float minVerticalAngle = -80f;
        [SerializeField] private float maxVerticalAngle = 80f;

        /// <summary>
        /// Set by future multiplayer/ownership code. Only the local player's controller
        /// should read device input; remote players will have this set to false.
        /// </summary>
        public bool IsLocalPlayer { get; set; } = true;

        /// <summary>
        /// Set by PlayerInteractor while a modal UI panel (e.g. the order board) owns the
        /// cursor. While true, this controller touches neither the cursor nor movement/look,
        /// so it can never fight the panel for the cursor lock state.
        /// </summary>
        public bool IsInputSuspended { get; set; }

        /// <summary>
        /// Set by PlayerInteractor during machine placement mode. Movement/look stay fully
        /// active (the player can walk around to find a spot) but Escape/click must NOT
        /// toggle the cursor lock here, since MachinePlacementController owns Escape (cancel)
        /// and left-click (confirm) for the duration — without this, HandleCursorToggle would
        /// also react to the same Escape press and unlock the cursor after placement ends.
        /// </summary>
        public bool SuppressCursorToggle { get; set; }

        private CharacterController _controller;
        private PlayerInputActions _input;
        private Vector3 _velocity;
        private float _verticalRotation;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Enable();
            LockCursor();
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        private void OnDestroy()
        {
            _input.Dispose();
        }

        private void Update()
        {
            if (!IsLocalPlayer || IsInputSuspended)
            {
                return;
            }

            if (!SuppressCursorToggle)
            {
                HandleCursorToggle();
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            _input.Poll();

            ApplyLook();
            ApplyMovement();
        }

        private void HandleCursorToggle()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                UnlockCursor();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                     Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ApplyLook()
        {
            Vector2 look = _input.LookInput * mouseSensitivity;

            transform.Rotate(Vector3.up * look.x);

            _verticalRotation -= look.y;
            _verticalRotation = Mathf.Clamp(_verticalRotation, minVerticalAngle, maxVerticalAngle);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
            }
        }

        private void ApplyMovement()
        {
            bool grounded = _controller.isGrounded;
            if (grounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            float currentSpeed = _input.SprintHeld ? sprintSpeed : walkSpeed;
            Vector3 moveDirection = transform.right * _input.MoveInput.x + transform.forward * _input.MoveInput.y;
            Vector3 horizontalMotion = moveDirection * currentSpeed;

            if (grounded && _input.JumpPressedThisFrame)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            _velocity.y += gravity * Time.deltaTime;

            Vector3 motion = (horizontalMotion + Vector3.up * _velocity.y) * Time.deltaTime;
            _controller.Move(motion);
        }
    }
}
