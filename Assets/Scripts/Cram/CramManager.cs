using UnityEngine;
using System.Collections.Generic;
using Shift25.Managers;
using Shift25.Gameplay;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Shift25.Managers
{
    // [Singleton Pattern] Simulates stock depletion to force player into the backroom.
    public class CramManager : MonoBehaviour
    {
        public static CramManager Instance { get; private set; }

        [SerializeField] private List<ShelfController> allShelves;
        private List<ShelfController> _emptyShelves = new List<ShelfController>();
        private CancellationTokenSource _cts;

        public bool HasEmptyShelves => _emptyShelves.Count > 0;

        private void Awake() { Instance = this; _cts = new CancellationTokenSource(); }

        private void Start() => ShelfDepletionLoop(_cts.Token).Forget();

        private async UniTaskVoid ShelfDepletionLoop(CancellationToken token)
        {
            await UniTask.Yield(); 
            while (!token.IsCancellationRequested)
            {
                var phase = GamePhaseManager.Instance.CurrentPhase;
                
                // [Logic Guard] Feature toggle check
                if (phase == null || !phase.enableCram) { await UniTask.Delay(2000); continue; }

                float delay = Random.Range(phase.minShelfDepletionInterval, phase.maxShelfDepletionInterval);
                await UniTask.Delay((int)(delay * 1000), cancellationToken: token);

                TryEmptyRandomShelf();
            }
        }

        private void TryEmptyRandomShelf()
        {
            var fullShelves = allShelves.FindAll(s => !s.IsEmpty);
            if (fullShelves.Count > 0)
            {
                var target = fullShelves[Random.Range(0, fullShelves.Count)];
                target.SetEmpty();
                _emptyShelves.Add(target);
                PressureManager.Instance.AddPressure(2f);
            }
        }

        public void ReportShelfFilled(ShelfController shelf) => _emptyShelves.Remove(shelf);
    }
}