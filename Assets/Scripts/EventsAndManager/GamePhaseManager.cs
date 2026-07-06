using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Shift25.Gameplay;

namespace Shift25.Managers
{
    // [Singleton Pattern] Orchestrates shift phases and ensures narrative beats play when NPCs are present.
    public class GamePhaseManager : MonoBehaviour
    {
        public static GamePhaseManager Instance { get; private set; }

        [Header("Phase Master List")]
        [SerializeField] private List<PhaseSettings> allPhases;
        
        private PhaseSettings _currentPhase;
        private int _index = 0;
        private CancellationTokenSource _phaseCts;
        private bool _canStart = false; 

        public PhaseSettings CurrentPhase => _currentPhase;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void BeginActualShift() => _canStart = true;

        private void Start() => MainShiftLoop().Forget();

        private async UniTaskVoid MainShiftLoop()
        {
            // Wait for the eye-blink sequence to finish
            await UniTask.WaitUntil(() => _canStart);
            await UniTask.Yield(); 

            while (_index < allPhases.Count)
            {
                _currentPhase = allPhases[_index];
                _phaseCts?.Cancel();
                _phaseCts = new CancellationTokenSource();

                Debug.Log($"<color=orange>[Phase]</color> Entering Phase: {_currentPhase.phaseNumber}");

                if (_currentPhase.phaseStartDialogue != null)
                    NarrativeManager.Instance.DisplayMessage(_currentPhase.phaseStartDialogue).Forget();

                GameEvents.RaisePhaseChanged(_currentPhase.phaseNumber);

                // [Algorithm] Start the pacing logic that waits for NPCs
                RunGuaranteedContextualNarrative(_currentPhase, _phaseCts.Token).Forget();

                if (_currentPhase.phaseNumber == 5)
                    MentalCollapseManager.Instance.ForceImmediateCollapse().Forget();

                float timer = 0;
                while (timer < _currentPhase.durationInSeconds)
                {
                    timer += Time.deltaTime;
                    await UniTask.Yield();
                }

                _index++;
            }

            // Force Mental Collapse 100% at the end of the shift
            PressureManager.Instance.AddPressure(999f);
        }

        // [Algorithm: Pacing & Context Guard] 
        // Plays every dialogue in the pool, but pauses if the store is empty.
        private async UniTaskVoid RunGuaranteedContextualNarrative(PhaseSettings settings, CancellationToken token)
        {
            if (settings.randomPhaseDialogues == null || settings.randomPhaseDialogues.Count == 0) return;

            float interval = settings.durationInSeconds / (settings.randomPhaseDialogues.Count + 1);
            
            // Create a temporary shuffled pool for variety
            List<Shift25.Gameplay.DialogueData> pool = new List<Shift25.Gameplay.DialogueData>(settings.randomPhaseDialogues);
            for (int i = 0; i < pool.Count; i++) {
                var temp = pool[i];
                int r = Random.Range(i, pool.Count);
                pool[i] = pool[r];
                pool[r] = temp;
            }

            foreach (var dialogue in pool)
            {
                // Wait for the average calculated interval (with 20% variance for natural feel)
                float waitTime = interval * Random.Range(0.8f, 1.2f);
                await UniTask.Delay((int)(waitTime * 1000), cancellationToken: token);

                if (token.IsCancellationRequested) return;

                // [Context Guard] Wait until at least one NPC is in the store before showing the text
                // This makes the dialogue feel like it's coming from an actual person in the scene.
                await UniTask.WaitUntil(() => NPCSpawner.Instance.ActiveNPCCount > 0 || token.IsCancellationRequested);

                if (token.IsCancellationRequested) return;

                NarrativeManager.Instance.DisplayMessage(dialogue).Forget();
            }
        }
    }
}