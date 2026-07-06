using UnityEngine;
using UnityEngine.InputSystem.UI;
using Shift25.Managers;

namespace Shift25.Managers
{
    // [Singleton Pattern] Manages UI focus and prevents background overlays from blocking input.
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject crosshairUI;
        [SerializeField] private CanvasGroup microwaveCanvasGroup;
        [SerializeField] private CanvasGroup mentalEffectGroup; // Blackout & Eyes

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Initial state: Game mode
            SetUIMode(false);
        }

        // [Logic] Precise control over mouse interaction and cursor state.
        public void SetUIMode(bool isUiActive)
        {
            Cursor.visible = isUiActive;
            Cursor.lockState = isUiActive ? CursorLockMode.None : CursorLockMode.Locked;

            if (crosshairUI != null) crosshairUI.SetActive(!isUiActive);

            // [Critical Fix] Ensure the correct panel blocks or allows raycasts
            if (microwaveCanvasGroup != null)
            {
                microwaveCanvasGroup.blocksRaycasts = isUiActive;
                microwaveCanvasGroup.interactable = isUiActive;
            }

            // Always ensure the mental collapse overlays don't block the slider
            if (mentalEffectGroup != null)
            {
                mentalEffectGroup.blocksRaycasts = false;
                mentalEffectGroup.interactable = false;
            }
        }
    }
}