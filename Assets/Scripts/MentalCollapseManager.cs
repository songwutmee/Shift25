using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shift25.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Threading;

namespace Shift25.Managers
{
    // [Singleton Pattern] Orchestrates the visual breakdown and the final "25:00" sequence.
    public class MentalCollapseManager : MonoBehaviour
    {
        public static MentalCollapseManager Instance { get; private set; }

        [Header("UI Overlays")]
        [SerializeField] private CanvasGroup blackoutPanel; // The black screen for transitions
        [SerializeField] private CanvasGroup eyeBorderGroup; // Container for the 8-frame eye animation
        [SerializeField] private Image eyeBorderDisplay;   // Target image for sprite swapping
        [SerializeField] private Sprite[] eyeFrames;       // 8 PNG frames for the eye animation

        [Header("Ending UI")]
        [SerializeField] private TextMeshProUGUI clockEndingText; // Big text for the 24:00 -> 25:00 shift
        [SerializeField] private string menuSceneName = "MainMenu";

        [Header("Renderer Control")]
        [SerializeField] private Material brokenWorldMat; // The Red/Black/White 1-bit shader material

        // [State Property] Public access so other systems know the world has collapsed.
        public bool IsCollapsed { get; private set; } = false;
        
        private bool _isEndingTriggered = false;
        private CancellationTokenSource _animCts;

        private void Awake()
        {
            // [Singleton] Setup instance and ensure cleanup
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // [Critical Fix] Reset persistent material values on start to prevent Red Screen bug.
            if (brokenWorldMat != null) brokenWorldMat.SetFloat("_RedAmount", 0f);
            
            ResetUI();
        }

        private void OnEnable() => GameEvents.OnPressureChanged += CheckCondition;
        private void OnDisable() { GameEvents.OnPressureChanged -= CheckCondition; _animCts?.Cancel(); }

        private void ResetUI()
        {
            if (blackoutPanel != null) { blackoutPanel.alpha = 0f; blackoutPanel.blocksRaycasts = false; }
            if (eyeBorderGroup != null) eyeBorderGroup.alpha = 0f;
            if (clockEndingText != null) clockEndingText.gameObject.SetActive(false);
            IsCollapsed = false;
            _isEndingTriggered = false;
        }

        // [Public API] Forces the world to shift into Red/Eye mode (Used for Phase 5 start).
        public async UniTaskVoid ForceImmediateCollapse()
        {
            if (IsCollapsed) return;
            IsCollapsed = true;

            // [Sequence] Smoothly fade to black -> Change Shader -> Reveal Red World
            await FadeCanvas(blackoutPanel, 1f, 1.5f);
            
            if (brokenWorldMat != null) brokenWorldMat.SetFloat("_RedAmount", 1.0f);
            
            // Start the 8-frame eye animation loop
            _animCts = new CancellationTokenSource();
            PlayEyeLoop(_animCts.Token).Forget();
            
            eyeBorderGroup.alpha = 1f;

            await UniTask.Delay(1000); // Psychological pause in darkness
            await FadeCanvas(blackoutPanel, 0f, 2.0f);
            Debug.Log("[Collapse] Reality Overwritten.");
        }

        private void CheckCondition(float current, float max)
        {
            if (_isEndingTriggered) return;

            // Trigger reality shift at 70% if not already collapsed
            if (!IsCollapsed && current >= max * 0.7f)
                ForceImmediateCollapse().Forget();

            // Trigger final clock sequence at 100%
            if (current >= max)
            {
                _isEndingTriggered = true;
                ExecuteFinalEnding().Forget();
            }
        }

        // [UniTask Sequencing] The dramatic finale: 24:00 to 25:00 transition.
        private async UniTaskVoid ExecuteFinalEnding()
        {
            Debug.Log("[Ending] The 25th Hour is arriving.");
            
            // Step 1: Slow Permanent Blackout
            blackoutPanel.blocksRaycasts = true;
            await FadeCanvas(blackoutPanel, 1f, 2.5f);

            // [Logic] Freeze world simulation to focus on UI
            Time.timeScale = 0; 
            await UniTask.Delay(1500, ignoreTimeScale: true);

            // Step 2: Show "24:00" and make it blink (White)
            clockEndingText.gameObject.SetActive(true);
            clockEndingText.color = Color.white;
            for (int i = 0; i < 4; i++)
            {
                clockEndingText.text = "24:00";
                await UniTask.Delay(500, ignoreTimeScale: true);
                clockEndingText.text = "";
                await UniTask.Delay(500, ignoreTimeScale: true);
            }

            // Step 3: The Surreal Shift to "25:00" (Red)
            clockEndingText.text = "25:00";
            clockEndingText.color = Color.red;
            
            await UniTask.Delay(5000, ignoreTimeScale: true); // Let the player stare at the horror

            // Step 4: Final Fade and Menu Load
            Time.timeScale = 1;
            SceneManager.LoadScene(menuSceneName);
        }

        private async UniTaskVoid PlayEyeLoop(CancellationToken token)
        {
            int f = 0;
            try {
                while (!token.IsCancellationRequested)
                {
                    if (eyeFrames.Length > 0 && eyeBorderDisplay != null)
                    {
                        eyeBorderDisplay.sprite = eyeFrames[f];
                        f = (f + 1) % eyeFrames.Length;
                    }
                    // Jerky frame rate (approx 12 FPS) for PSX jitter feel
                    await UniTask.Delay(80, cancellationToken: token);
                }
            } catch { }
        }

        private async UniTask FadeCanvas(CanvasGroup group, float target, float duration)
        {
            float start = group.alpha;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(start, target, elapsed / duration);
                await UniTask.Yield();
            }
            group.alpha = target;
        }
    }
}