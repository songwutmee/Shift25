using UnityEngine;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    // [Interface Realization] Handles item logic with high-visibility outlines.
    [RequireComponent(typeof(Rigidbody), typeof(Outline))]
    public class ScannableItem : MonoBehaviour
    {
        private ScanItemData _data;
        private float _timer = 0f;
        private bool _isReady = false;
        private bool _processed = false;
        private Outline _outline;

        [Header("Visuals")]
        [SerializeField] private Color processingColor = Color.red;
        [SerializeField] private Color readyToClickColor = Color.green;
        [SerializeField] private DialogueData scanComplaintDialogue;

        public void Initialize(ScanItemData data)
        {
            _data = data;
            if (TryGetComponent<Outline>(out _outline))
            {
                _outline.enabled = false;
                _outline.OutlineColor = processingColor;
                _outline.OutlineWidth = 8f;
            }
        }

        private void Update()
        {
            if (_processed) return;
            CheckHover();
        }

        private void CheckHover()
        {
            if (Camera.main == null || _outline == null) return;

            // [Physics Logic] Precise mouse raycast to detect hovering
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool isHovered = false;

            if (Physics.Raycast(ray, out RaycastHit hit, 5.0f))
            {
                if (hit.collider.gameObject == this.gameObject) isHovered = true;
            }

            if (isHovered)
            {
                _outline.enabled = true;
                UpdateProgress();
                UpdateOutlineVisuals();
            }
            else
            {
                _outline.enabled = false;
                _timer = 0f;
                _isReady = false;
            }
        }

        private void UpdateProgress()
        {
            if (_isReady) return;
            _timer += Time.deltaTime;
            if (_timer >= _data.baseScanTime) _isReady = true;
        }

        private void UpdateOutlineVisuals()
        {
            // [State Sync] Access MentalCollapseManager to adjust visibility in red world
            bool isBroken = MentalCollapseManager.Instance != null && MentalCollapseManager.Instance.IsCollapsed;

            if (_isReady)
            {
                _outline.OutlineColor = isBroken ? Color.green : readyToClickColor;
                _outline.OutlineWidth = isBroken ? 12f : 10f; // Force thickness for PSX look
            }
            else
            {
                _outline.OutlineColor = processingColor;
                _outline.OutlineWidth = 8f;
            }
        }

        public void OnClickAction()
        {
            if (_processed) return;

            if (!_isReady)
            {
                PressureManager.Instance.AddPressure(3f);
                return;
            }

            _processed = true;
            _outline.enabled = false;

            // [Audio Integration] Play scan success beep
            if (AudioManager.Instance != null) AudioManager.Instance.PlayScanSFX();

            PressureManager.Instance.AddPressure(1f);
            GameEvents.RaiseActionPerformed(1);
            ScanManager.Instance.ReportItemScanned(this.gameObject);
            Destroy(gameObject);
        }
    }
}