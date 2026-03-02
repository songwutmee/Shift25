using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shift25.Managers;
using Cysharp.Threading.Tasks;

namespace Shift25.Gameplay
{
    public class VoidDoor : MonoBehaviour, IInteractable
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private string boolParameter = "IsOpen"; 

        private bool _isOpen = false;

        // Dynamic prompt based on door state
        public string InteractionPrompt => _isOpen ? "Press E to Close Door" : "Press E to Open Door";

        public void Interact()
        {
            // Prevent interaction if player is busy (optional)
            if (PlayerStateManager.Instance.CurrentState == PlayerStateManager.PlayerState.Interacting) return;
            
            ToggleDoor().Forget();
        }

        private async UniTaskVoid ToggleDoor()
        {
            _isOpen = !_isOpen;

            // Update the boolean to trigger transition
            if (doorAnimator != null)
            {
                doorAnimator.SetBool(boolParameter, _isOpen);
            }

            // Wait for half a second for the door to actually open before checking for Yeet
            await UniTask.Delay(500);

            if (_isOpen)
            {
                CheckForTrashInHand();
            }
        }

        private void CheckForTrashInHand()
        {
            // Look for TrashPickup script attached to children of Main Camera
            var mainCam = Camera.main;
            if (mainCam == null) return;

            TrashPickup heldTrash = mainCam.GetComponentInChildren<TrashPickup>();

            if (heldTrash != null)
            {
                // Trigger the throwing action if trash is found
                heldTrash.YeetIntoVoid();
                Debug.Log("[VoidDoor] Trash detected and yeeted into the void.");
            }
        }
    }
}