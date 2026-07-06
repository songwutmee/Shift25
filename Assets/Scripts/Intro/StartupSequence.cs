using UnityEngine;
using Cysharp.Threading.Tasks;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    // [Game Feel] Orchestrates the start-of-shift blinking sequence and delayed audio.
    public class StartupSequence : MonoBehaviour
    {
        [SerializeField] private CanvasGroup blinkOverlay;

        private async void Start()
        {
            // [State Logic] Freeze all physics and AI during the blink intro.
            Time.timeScale = 0f;
            if (blinkOverlay != null) blinkOverlay.alpha = 1f;

            // Wait for managers to wake up
            await UniTask.Delay(1000, ignoreTimeScale: true);

            // Blinking sequence (Open -> Close -> Repeat)
            for (int i = 0; i < 3; i++)
            {
                await FadeLid(0.4f, 0.4f);
                await FadeLid(1f, 0.2f);
                await UniTask.Delay(300, ignoreTimeScale: true);
            }

            // [Step 1] Open eyes fully. Shift is now visible.
            await FadeLid(0f, 2.5f);
            
            // [Step 2] Resume time scale to 1. Game logic starts.
            Time.timeScale = 1f;
            Debug.Log("[Startup] World logic resumed.");

            // [Step 3] 1.2 Second psychological delay before the sensory input (BGM/Voice) starts.
            await UniTask.Delay(1200);

            // [Step 4] Trigger Audio and Shift Progress via Singletons.
            if (AudioManager.Instance != null) AudioManager.Instance.PlayNormalBGM();
            if (GamePhaseManager.Instance != null) GamePhaseManager.Instance.BeginActualShift();

            Debug.Log("[Startup] Audio and Shift initialized.");
            
            // Cleanup the sequence controller.
            gameObject.SetActive(false);
        }

        private async UniTask FadeLid(float target, float duration)
        {
            if (blinkOverlay == null) return;
            float start = blinkOverlay.alpha;
            float time = 0;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                blinkOverlay.alpha = Mathf.Lerp(start, target, time / duration);
                await UniTask.Yield();
            }
            blinkOverlay.alpha = target;
        }
    }
}