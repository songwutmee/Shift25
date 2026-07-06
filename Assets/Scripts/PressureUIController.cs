using UnityEngine;
using UnityEngine.UI;
using Shift25.Managers;
using Cysharp.Threading.Tasks;

namespace Shift25.Gameplay
{
    // [Observer Pattern] Updated to break the UI at 70% instead of 100% for narrative impact.
    public class PressureUIController : MonoBehaviour
    {
        [Header("Meter Visuals")]
        [SerializeField] private Image meterDisplay; 
        [SerializeField] private Sprite brokenSprite; 

        [Header("Pointer Logic")]
        [SerializeField] private RectTransform pointerArrow; 
        [SerializeField] private float bottomY = -150f; 
        [SerializeField] private float topY = 150f;    
        [SerializeField] private float moveSmoothness = 0.2f;

        private float _currentDisplayedPercentage = 0f;
        private bool _isBroken = false;

        private void OnEnable() => GameEvents.OnPressureChanged += SyncUI;
        private void OnDisable() => GameEvents.OnPressureChanged -= SyncUI;

        private void SyncUI(float current, float max)
        {
            if (_isBroken) return;

            float targetPercentage = Mathf.Clamp01(current / max);
            UpdatePointerPosition(targetPercentage).Forget();

            // [Logic] Force break UI at 70% stress or Phase 5
            if (targetPercentage >= 0.7f)
            {
                ApplyBrokenState();
            }
        }

        private async UniTaskVoid UpdatePointerPosition(float targetPerc)
        {
            float elapsed = 0;
            float startPerc = _currentDisplayedPercentage;

            while (elapsed < moveSmoothness)
            {
                if (this == null || pointerArrow == null || _isBroken) break;

                elapsed += Time.deltaTime;
                _currentDisplayedPercentage = Mathf.Lerp(startPerc, targetPerc, elapsed / moveSmoothness);
                float calculatedY = Mathf.Lerp(bottomY, topY, _currentDisplayedPercentage);
                pointerArrow.anchoredPosition = new Vector2(pointerArrow.anchoredPosition.x, calculatedY);
                await UniTask.Yield();
            }
        }

        public void ApplyBrokenState()
        {
            if (_isBroken) return;
            _isBroken = true;

            // [Visual State Swap] Switch to broken graphics immediately at 70%
            if (meterDisplay != null) meterDisplay.sprite = brokenSprite;
            if (pointerArrow != null) pointerArrow.gameObject.SetActive(false);
            
            Debug.Log("[UI] Pressure threshold reached. UI Shattered.");
        }
    }
}