using UnityEngine;
using UnityEngine.InputSystem;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 4f; // Public for weight system
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float lookSensitivity = 0.1f;

        private CharacterController _controller;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _xRotation = 0f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OnMove(InputAction.CallbackContext context) => _moveInput = context.ReadValue<Vector2>();
        public void OnLook(InputAction.CallbackContext context) => _lookInput = context.ReadValue<Vector2>();

        private void Update()
        {
            // [CRITICAL FIX] Block all inputs if the game is paused (during blinking)
            if (Time.timeScale <= 0) return;

            bool canMove = PlayerStateManager.Instance.CurrentState == PlayerStateManager.PlayerState.Roaming ||
                           PlayerStateManager.Instance.CurrentState == PlayerStateManager.PlayerState.Locked;

            if (canMove)
            {
                HandleMovement();
                HandleRotation();
            }
        }

        private void HandleMovement()
        {
            Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            _controller.Move(move * moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            transform.Rotate(Vector3.up * _lookInput.x * lookSensitivity);
            _xRotation -= _lookInput.y * lookSensitivity;
            _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
            cameraTarget.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        }

        public Vector3 GetCurrentVelocity() => _controller.velocity;
        public Vector2 GetLocalInput() => _moveInput;
    }
}