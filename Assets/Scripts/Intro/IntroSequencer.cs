using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Shift25.Managers
{
    // [UniTask Sequencing] Manages the novel-style prose and transition to gameplay.
    public class IntroSequencer : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI proseText;
        [SerializeField] private CanvasGroup proseGroup;

        [Header("Content")]
        [SerializeField, TextArea(10, 20)] private string storyProse;

        private async void Start()
        {
            proseGroup.alpha = 0f;
            proseText.text = "";
            
            await UniTask.Delay(1000); // Breathe in the silence
            
            await FadeCanvas(1f, 2f);
            await TypeProse(storyProse);
            
            await UniTask.Delay(3000); // Let the weight of the words sink in
            
            await FadeCanvas(0f, 2f);
            SceneManager.LoadScene("MainStore");
        }

        // [Algorithm] Typewriter effect that ignores novel tags like <color>
        private async UniTask TypeProse(string fullText)
        {
            proseText.text = "";
            int i = 0;
            while (i < fullText.Length)
            {
                // Logic to skip rich text tags so they appear instantly
                if (fullText[i] == '<')
                {
                    while (fullText[i] != '>') { proseText.text += fullText[i]; i++; }
                    proseText.text += fullText[i]; i++;
                }
                else
                {
                    proseText.text += fullText[i];
                    i++;
                    await UniTask.Delay(40); // Standard reading speed
                }
            }
        }

        private async UniTask FadeCanvas(float target, float duration)
        {
            float start = proseGroup.alpha;
            float elapsed = 0;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                proseGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
                await UniTask.Yield();
            }
        }
    }
}
