using UnityEngine;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    // [Interface Implementation] Handles the shelf refill logic and visual state.
    public class ShelfController : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private GameObject productPlane; // The visual items
        
        private bool _isEmpty = false;
        public bool IsEmpty => _isEmpty;

        // [Logic] Check if player is holding a box using spatial query
        public string InteractionPrompt 
        {
            get {
                if (!_isEmpty) return "";
                
                // [Null Safety] Check if the player is holding a box
                HeldBox heldBox = Camera.main.GetComponentInChildren<HeldBox>();
                if (heldBox != null) return "Press E to Refill Products";
                
                return "Shelf is Empty (Need Supply Box)";
            }
        }

        private void Awake()
        {
            // [Professional Practice] Ensure the product plane doesn't block the interaction raycast
            if (productPlane != null)
            {
                // Force the visual plane to Ignore Raycast so we always hit the main shelf collider
                productPlane.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        public void SetEmpty()
        {
            _isEmpty = true;
            if (productPlane != null) productPlane.SetActive(false);
            Debug.Log($"[Shelf] {gameObject.name} is now empty.");
        }

        public void Interact()
        {
            if (!_isEmpty) return;

            // [Dependency Injection Concept] Look for the HeldBox script in the camera's hierarchy
            HeldBox heldBox = Camera.main.GetComponentInChildren<HeldBox>();

            if (heldBox != null)
            {
                Refill(heldBox);
            }
            else
            {
                Debug.LogWarning("[Shelf] You need to carry a supply box to refill this!");
            }
        }

        private void Refill(HeldBox box)
        {
            _isEmpty = false;
            if (productPlane != null) productPlane.SetActive(true);
            
            // [Logic] Tell the box to destroy itself and reset player speed
            box.ConsumeBox();
            
            // Notify the central manager
            CramManager.Instance.ReportShelfFilled(this);
            
            Debug.Log("[Shelf] Refill successful. Items are back in stock.");
        }
    }
}