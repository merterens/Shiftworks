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
        /// active (the player can walk around to find a spot) but left-click must NOT
        /// re-lock the cursor here, since MachinePlacementController owns left-click
        /// (confirm placement) for the duration — without this, HandleCursorToggle would
        /// also react to that same click.
        /// </summary>
        public bool SuppressCursorToggle { get; set; }

        /// <summary>(Stage 10) Current vertical look angle, for SaveManager to capture —
        /// read-only mirror of the same _verticalRotation ApplyLook/SetLookRotation write.</summary>
        public float CurrentPitch => _verticalRotation;

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

        /// <summary>
        /// (Stage 10 fix) Escape used to be handled here too (a bare cursor-unlock) — removed.
        /// With two components independently reacting to the same key press, whichever one's
        /// Update() Unity happened to call first would win: if this ran before
        /// PlayerInteractor's own Update(), the cursor was already unlocked by the time
        /// PlayerInteractor checked Cursor.lockState, so it took its "not locked, do nothing"
        /// early-out and never reached its own Escape handling — the Save/Load menu then
        /// never opened, only the cursor appeared, exactly the "sadece mouse imleci geliyor"
        /// symptom this fixes. PlayerInteractor.HandleEscapeKey is now the single place that
        /// decides what Escape does (open the menu, or — if no menu is wired — fall back to
        /// unlocking the cursor itself), the same way it already exclusively owns E/G.
        /// </summary>
        private void HandleCursorToggle()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        /// <summary>(Stage 10) Restores a saved look direction — sets both the visible
        /// cameraPivot rotation AND the internal _verticalRotation it's derived from, so the
        /// very next mouse-look update continues smoothly from here instead of snapping back
        /// to whatever _verticalRotation held before (0 by default on a fresh scene load).
        /// Yaw is applied to this transform directly, exactly like ApplyLook's own
        /// transform.Rotate does, just as an absolute set instead of a delta.</summary>
        public void SetLookRotation(float yaw, float pitch)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            _verticalRotation = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
