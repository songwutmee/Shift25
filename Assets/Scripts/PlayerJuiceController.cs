using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    // [Architectural Pattern: Functional Component] 
    // Separates visual 'Juice' logic from the physical movement logic.
    public class PlayerJuiceController : MonoBehaviour
    {
        [SerializeField] private MovementJuiceData juiceData;
        [SerializeField] private Transform cameraHolder; // The CameraTarget we created before
        [SerializeField] private PlayerController playerController;

        private float _timer = 0f;
        private Vector3 _defaultPos;
        private float _currentBobIntensity = 1f;

        private void Awake()
        {
            if (cameraHolder != null) _defaultPos = cameraHolder.localPosition;
        }

        private void Update()
        {
            // [State Pattern Check] Only apply juice when player is in movement-enabled states
            var state = PlayerStateManager.Instance.CurrentState;
            if (state == PlayerStateManager.PlayerState.Roaming || state == PlayerStateManager.PlayerState.Locked)
            {
                HandleHeadbob();
                HandleCameraTilt();
            }
            else
            {
                ResetCameraEffects();
            }
        }

        // [Algorithm: Sine-Wave Bobbing]
        // Creates a procedural bobbing motion based on movement velocity.
        private void HandleHeadbob()
        {
            // Get speed magnitude from our PlayerController (ensure it's public)
            float speed = playerController.GetCurrentVelocity().magnitude;

            if (speed > 0.1f)
            {
                // [Logic] Increase timer based on movement speed
                _timer += Time.deltaTime * juiceData.bobSpeed;

                // [Algorithm] Sinusoidal movement for natural head sway
                // Y-axis = Up/Down bobbing, X-axis = Subtle side-to-side sway
                float newY = _defaultPos.y + Mathf.Sin(_timer) * juiceData.bobAmount;
                float newX = _defaultPos.x + Mathf.Cos(_timer / 2) * juiceData.bobAmount;

                // [Pressure Integration] Distort bobbing intensity if stressed
                // As pressure increases, the movement becomes more erratic
                ApplyPressureModifiers();
                
                cameraHolder.localPosition = new Vector3(newX, newY, _defaultPos.z);
            }
            else
            {
                // Smoothly return to center when stopped
                _timer = 0;
                cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, _defaultPos, Time.deltaTime * 5f);
            }
        }

        // [Algorithm: Procedural Tilt]
        // Tilts the camera slightly when strafing to increase the sense of weight.
        private void HandleCameraTilt()
        {
            float moveX = playerController.GetLocalInput().x; // Left/Right input
            float targetTilt = -moveX * juiceData.tiltAmount;
            
            Quaternion targetRot = Quaternion.Euler(cameraHolder.localRotation.eulerAngles.x, 
                                                    cameraHolder.localRotation.eulerAngles.y, 
                                                    targetTilt);
                                                    
            cameraHolder.localRotation = Quaternion.Lerp(cameraHolder.localRotation, targetRot, Time.deltaTime * juiceData.tiltSpeed);
        }

        private void ApplyPressureModifiers()
        {
            // [Observer Pattern Concept] Accessing central pressure manager to modify feel
            // If we're at high pressure, double the bobbing amount to simulate panic
            // float pressurePerc = PressureManager.Instance.GetPressurePercentage();
            // _currentBobIntensity = 1f + (pressurePerc * 1.5f);
        }

        private void ResetCameraEffects()
        {
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, _defaultPos, Time.deltaTime * 5f);
            cameraHolder.localRotation = Quaternion.Lerp(cameraHolder.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
    }
}