using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Shift25.Managers
{
    // [Narrative Design] Orchestrates the realistic, heavy atmosphere of the intro.
    public class IntroManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI narrativeText;
        [SerializeField] private CanvasGroup screenFader;

        [Header("Audio")]
        [SerializeField] private AudioSource typewriterSource; 
        [SerializeField] private AudioClip typeClip;

        [Header("Settings")]
        [SerializeField] private string storeSceneName = "MainStore";

        // [Narrative Overhaul] Focuses on modern poverty and exhausted labor. 
        // Ends exactly with "Shift25". No em-dashes used.
        private string _storyContent = 
            "My eyes open before the alarm does. The darkness in this room is heavy, " +
            "smelling like stale coffee and cheap detergent. The bank does not sleep. " +
            "Their letters are stacked on the kitchen table like small white tombstones, " +
            "each one reminding me that I am merely an extension of their interest rates. " +
            "Every breath I take feels like a transaction. My mother's cough through " +
            "the thin walls is a sound I cannot afford to hear anymore. " +
            "The doctors talk about treatments, but they don't see the numbers on my paycheck. " +
            "I walk to work through the same grey streets, under a sun that offers no warmth, " +
            "just another day of selling my soul for seventy baht an hour. " +
            "The fluorescent lights of the store wait for me. They don't illuminate, " +
            "they only expose how much of me is left. I am a ghost in a blue uniform, " +
            "standing behind a counter that feels more like a cage every night. " +
            "No more dreams. No more potential. Just the endless beep of the scanner " +
            "and the weight of a life I didn't choose. " +
            "It is time to report for <color=red>Shift25</color>.";

        private async void Start()
        {
            screenFader.alpha = 1f;
            narrativeText.text = "";
            
            await UniTask.Delay(1500);
            await Fade(0f, 2.5f); 

            // [Algorithm] Typewriter sequence with audio triggering
            foreach (char c in _storyContent) {
                narrativeText.text += c;
                
                // Trigger sound effect for each character
                if (typewriterSource != null && typeClip != null)
                {
                    typewriterSource.PlayOneShot(typeClip, 0.3f);
                }

                // Variable delay for a more natural typing feel
                int delay = (c == '.' || c == ',') ? 300 : 35;
                await UniTask.Delay(delay); 
            }

            // Let the player soak in the final words
            await UniTask.Delay(5000);

            await Fade(1f, 2.5f);
            SceneManager.LoadScene(storeSceneName);
        }

        private async UniTask Fade(float target, float duration)
        {
            float start = screenFader.alpha;
            float time = 0;
            while (time < duration) {
                time += Time.deltaTime;
                screenFader.alpha = Mathf.Lerp(start, target, time / duration);
                await UniTask.Yield();
            }
            screenFader.alpha = target;
        }
    }
}