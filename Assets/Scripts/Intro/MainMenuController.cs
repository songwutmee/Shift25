using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using Shift25.Managers;

namespace Shift25.Managers
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Groups")]
        [SerializeField] private CanvasGroup menuUIGroup; 
        [SerializeField] private CanvasGroup sceneFader;  

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;   // Background music for menu
        [SerializeField] private AudioSource sfxSource;   // For the click sound
        [SerializeField] private AudioClip startClip;

        private bool _isTransitioning = false;
        private CancellationTokenSource _cts;

        private void Awake() 
        {
            if(sceneFader != null) sceneFader.alpha = 0f;
            _cts = new CancellationTokenSource();
        }

        private void Start() => PulseTextEffect(_cts.Token).Forget();

        private void Update()
        {
            bool anyInput = Keyboard.current.anyKey.wasPressedThisFrame || 
                            (Pointer.current != null && Pointer.current.press.wasPressedThisFrame);

            if (!_isTransitioning && anyInput)
            {
                // Play click sound upon starting
                if (sfxSource != null && startClip != null) sfxSource.PlayOneShot(startClip);
                BeginIntroSequence().Forget();
            }
        }

        private async UniTaskVoid PulseTextEffect(CancellationToken token)
        {
            try {
                while (!token.IsCancellationRequested) {
                    // [Algorithm] Sine wave modified for lower opacity
                    // Oscillates between 0.05 and 0.4 for a very faint, bleak look.
                    float alpha = (Mathf.Sin(Time.time * 1.5f) * 0.175f) + 0.225f;
                    menuUIGroup.alpha = alpha;
                    await UniTask.Yield();
                }
            } catch { }
        }

        private async UniTaskVoid BeginIntroSequence()
        {
            _isTransitioning = true;
            _cts.Cancel(); 

            await FadeCanvas(menuUIGroup, 0f, 0.5f);
            await FadeCanvas(sceneFader, 1f, 1.5f);

            SceneManager.LoadScene("IntroScene");
        }

        private async UniTask FadeCanvas(CanvasGroup group, float target, float duration)
        {
            float start = group.alpha;
            float time = 0;
            while (time < duration) {
                time += Time.deltaTime;
                group.alpha = Mathf.Lerp(start, target, time / duration);
                await UniTask.Yield();
            }
            group.alpha = target;
        }
    }
}