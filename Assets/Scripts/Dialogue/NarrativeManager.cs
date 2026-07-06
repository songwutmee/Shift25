using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using Shift25.Managers;

namespace Shift25.Managers
{
    // [Singleton Pattern] Controls the subtitle UI and short audio bursts.
    public class NarrativeManager : MonoBehaviour
    {
        public static NarrativeManager Instance { get; private set; }

        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private CanvasGroup subtitleGroup;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioClip talkBeep;
        [SerializeField] private float soundBurstDuration = 0.3f; // [Data] strictly play for 0.3s

        private CancellationTokenSource _msgCts;
        private bool _isDisplaying = false;

        private void Awake()
        {
            // [Singleton] Ensure only one manager exists across scenes.
            if (Instance == null) {
                Instance = this;
                if (transform.parent != null) transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        public async UniTaskVoid DisplayMessage(Shift25.Gameplay.DialogueData data, float duration = 4f)
        {
            if (data == null || subtitleText == null) return;

            // Wait if a previous message is still playing
            await UniTask.WaitUntil(() => !_isDisplaying);
            _isDisplaying = true;

            // Reset cancellation token for the new message
            _msgCts?.Cancel();
            _msgCts = new CancellationTokenSource();

            // Set UI content
            subtitleText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(data.textColor)}>{data.speakerName.ToUpper()}:</color> {data.GetRandomLine()}";
            
            // [UniTask] Start the 0.3s rapid audio burst.
            PlayShortVoiceBurst(_msgCts.Token).Forget();

            // [Sequence] Fade In -> Wait -> Fade Out
            await Fade(1f, 0.3f, _msgCts.Token);
            await UniTask.Delay((int)(duration * 1000), cancellationToken: _msgCts.Token);
            await Fade(0f, 0.5f, _msgCts.Token);

            _isDisplaying = false;
        }

        private async UniTaskVoid PlayShortVoiceBurst(CancellationToken token)
        {
            // [Algorithm] High-speed audio burst limited by a precise timer.
            float timer = 0;
            try {
                // Strictly stop when timer reaches 0.3s (soundBurstDuration)
                while (!token.IsCancellationRequested && timer < soundBurstDuration) {
                    if (voiceSource != null && voiceSource.isActiveAndEnabled && talkBeep != null) {
                        // Randomize pitch to make the 'gibberish' sound more distorted and surreal.
                        voiceSource.pitch = Random.Range(0.6f, 1.8f);
                        voiceSource.PlayOneShot(talkBeep, 0.4f);
                    }
                    
                    // Interval between beeps (40ms to 60ms)
                    int delayMs = Random.Range(40, 60);
                    timer += delayMs / 1000f; // Track elapsed time
                    
                    await UniTask.Delay(delayMs, cancellationToken: token);
                }
            } catch (System.OperationCanceledException) { 
                // Task was aborted correctly.
            }
        }

        private async UniTask Fade(float target, float time, CancellationToken token)
        {
            if (subtitleGroup == null) return;
            float start = subtitleGroup.alpha;
            float elapsed = 0;
            try {
                while (elapsed < time) {
                    elapsed += Time.unscaledDeltaTime;
                    subtitleGroup.alpha = Mathf.Lerp(start, target, elapsed / time);
                    await UniTask.Yield(token);
                }
            } catch { }
            subtitleGroup.alpha = target;
        }
    }
}