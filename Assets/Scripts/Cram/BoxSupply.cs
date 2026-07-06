using UnityEngine;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    public class BoxSupply : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private GameObject boxPrefab; 
        [SerializeField] private float boxWeightMultiplier = 0.5f;
        
        // [Logic] Set this to (100, 100, 100) if your model is very small, 
        // or (1, 1, 1) if it's already big enough.
        [SerializeField] private Vector3 scaleInHand = new Vector3(100f, 100f, 100f);

        public string InteractionPrompt 
        {
            get {
                if (CramManager.Instance == null) return "";
                return CramManager.Instance.HasEmptyShelves ? "Press E to Grab Supply Box" : "Stock is Full";
            }
        }

        public void Interact()
        {
            // [Debug] If you see the 'already carrying' log, it means a hidden box exists!
            if (Camera.main.GetComponentInChildren<HeldBox>() != null)
            {
                Debug.LogWarning("[BoxSupply] Logic blocked: A box script already exists under Camera.");
                return;
            }

            if (!CramManager.Instance.HasEmptyShelves) return;

            SpawnBoxInHand();
        }

        private void SpawnBoxInHand()
        {
            // [Factory Pattern] Instantiate the box
            GameObject boxInstance = Instantiate(boxPrefab);
            
            // [Layer Management] Force the held box to 'Ignore Raycast' layer 
            // so it doesn't block the player's vision/raycast.
            boxInstance.layer = LayerMask.NameToLayer("Ignore Raycast");

            HeldBox logic = boxInstance.GetComponent<HeldBox>();
            if (logic == null) logic = boxInstance.AddComponent<HeldBox>();
            
            // Apply your specific scale (100)
            boxInstance.transform.localScale = scaleInHand;
            
            logic.Initialize(boxWeightMultiplier);
            
            Debug.Log("[BoxSupply] Box spawned. Check Camera children in Hierarchy!");
        }
    }
}