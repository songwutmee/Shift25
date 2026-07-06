using UnityEngine;
using Shift25.Managers;
using Cysharp.Threading.Tasks;

namespace Shift25.Managers
{
    // [Singleton Pattern] Manages global audio. Must be a ROOT object in Hierarchy.
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM Channels")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip normalBGM;
        [SerializeField] private AudioClip stressedBGM;
        [SerializeField] private AudioClip brokenBGM;

        [Header("SFX Channels")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip interactClip;
        [SerializeField] private AudioClip scanSuccessClip;

        private void Awake()
        {
            // [Singleton Fix] Only work if this is a ROOT object.
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent != null) transform.SetParent(null); // Force to Root
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayNormalBGM()
        {
            if (bgmSource == null || normalBGM == null) return;
            bgmSource.clip = normalBGM;
            bgmSource.loop = true;
            bgmSource.Play();
            Debug.Log("<color=green>[Audio]</color> Normal BGM started playing.");
        }

        private void OnEnable() => GameEvents.OnPressureChanged += SyncMusicToPressure;
        private void OnDisable() => GameEvents.OnPressureChanged -= SyncMusicToPressure;

        private void SyncMusicToPressure(float current, float max)
        {
            if (bgmSource == null || !bgmSource.isPlaying) return;
            float ratio = current / max;
            if (ratio >= 1.0f) SwapBGM(brokenBGM);
            else if (ratio >= 0.7f) SwapBGM(stressedBGM);
        }

        private void SwapBGM(AudioClip newClip)
        {
            if (newClip == null || bgmSource.clip == newClip) return;
            bgmSource.clip = newClip;
            bgmSource.Play();
        }

        public void PlayInteractSFX() { if (sfxSource.isActiveAndEnabled) sfxSource.PlayOneShot(interactClip); }
        public void PlayScanSFX() { if (sfxSource.isActiveAndEnabled) sfxSource.PlayOneShot(scanSuccessClip); }
    }
}