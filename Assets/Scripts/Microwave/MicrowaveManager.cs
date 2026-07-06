using System.Collections.Generic;
using UnityEngine;
using Shift25.Managers;
using Shift25.Gameplay;
using Cysharp.Threading.Tasks;
using TMPro;
using Cinemachine;

namespace Shift25.Managers
{
    public class MicrowaveManager : MonoBehaviour
    {
        public static MicrowaveManager Instance { get; private set; }

        public enum MicrowaveState { Idle, SettingTime, Cooking, Done }
        public MicrowaveState CurrentState { get; private set; } = MicrowaveState.Idle;

        [Header("Camera & Visuals")]
        [SerializeField] private CinemachineVirtualCamera microwaveCam;

        [Header("UI References")]
        [SerializeField] private GameObject microwaveUIPanel;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip finishBeepClip; // [New] Assign Beep sound

        private MicrowaveRequestData _activeRequest;
        private float _userSelectedTime;

        public bool HasActiveRequest => _activeRequest != null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AssignPendingRequest(MicrowaveRequestData request)
        {
            _activeRequest = request;
            if (instructionText != null && _activeRequest != null)
                instructionText.text = $"Order: \"{_activeRequest.instructionPhrase}\"";
        }

        public void ActivateCamera() => microwaveCam.Priority = 20;
        public void DeactivateCamera() => microwaveCam.Priority = 0;

        public async UniTask StartSettingTime()
        {
            if (!HasActiveRequest) return;
            CurrentState = MicrowaveState.SettingTime;
            UIManager.Instance.SetUIMode(true);
            microwaveUIPanel.SetActive(true);

            await UniTask.WaitUntil(() => CurrentState == MicrowaveState.Cooking);

            microwaveUIPanel.SetActive(false);
            DeactivateCamera();
            UIManager.Instance.SetUIMode(false);
        }

        public void SubmitCookingTime(float sliderValue)
        {
            _userSelectedTime = sliderValue * 5.0f;
            CurrentState = MicrowaveState.Cooking;
        }

        public async UniTask RunCookingTimer(Animator doorAnimator)
        {
            if (doorAnimator != null) doorAnimator.SetBool("IsOpen", false);
            
            // [UniTask] Wait for cooking duration
            await UniTask.Delay((int)(_userSelectedTime * 1000));

            // [Audio] Play finish beep when Done
            if (sfxSource != null && finishBeepClip != null)
                sfxSource.PlayOneShot(finishBeepClip);

            CurrentState = MicrowaveState.Done;
        }

        public float GetResultAndReset()
        {
            if (_activeRequest == null) return 0;
            float pGain = EvaluateResult(_userSelectedTime);
            _activeRequest = null;
            CurrentState = MicrowaveState.Idle;
            return pGain;
        }
        
        private float EvaluateResult(float finalTime)
        {
            float pGain = 1f;
            DialogueData feedback = null;

            if (finalTime < _activeRequest.minAcceptableTime) {
                pGain = 3f;
                feedback = _activeRequest.tooColdDialogue;
            } else if (finalTime > _activeRequest.maxAcceptableTime) {
                pGain = 2f;
                feedback = _activeRequest.tooHotDialogue;
            }

            if (feedback != null) NarrativeManager.Instance.DisplayMessage(feedback).Forget();
            return pGain;
        }
    }
}