using UnityEngine;
using UnityEngine.Pool;
using Shift25.Managers;
using Shift25.Gameplay;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Shift25.Managers
{
    // [Singleton Pattern] Manages the accumulation of waste and triggers boss narrative.
    public class TrashManager : MonoBehaviour
    {
        public static TrashManager Instance { get; private set; }

        [Header("Configurations")]
        [SerializeField] private TrashData trashData;
        [SerializeField] private Transform[] spawnPoints;

        [Header("Narrative")]
        [SerializeField] private DialogueData bossAngryTrashDialogue; // Boss yells when cluttered

        // [Object Pooling] Prevents memory spikes during intense shift phases.
        private IObjectPool<GameObject> _trashPool;
        private List<GameObject> _activeTrashInRoom = new List<GameObject>();
        private CancellationTokenSource _cts;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _cts = new CancellationTokenSource();

            // [Factory Pattern] Initializing the Pool warehouse
            _trashPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(trashData.trashPrefab),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: 5,
                maxSize: 15
            );
        }

        private void Start() => TrashGenerationLoop(_cts.Token).Forget();

        private async UniTaskVoid TrashGenerationLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var currentPhase = GamePhaseManager.Instance.CurrentPhase;
                if (currentPhase == null || !currentPhase.enableTrash) { await UniTask.Delay(1000); continue; }

                // [Negligence Algorithm]
                if (_activeTrashInRoom.Count >= currentPhase.maxTrashInRoom)
                {
                    // [Narrative] Trigger Boss anger when work is neglected
                    if (Random.value < 0.3f) // 30% chance to remind each tick
                        NarrativeManager.Instance.DisplayMessage(bossAngryTrashDialogue).Forget();

                    PressureManager.Instance.AddPressure(1.0f);
                }

                float interval = Random.Range(currentPhase.minTrashSpawnInterval, currentPhase.maxTrashSpawnInterval);
                await UniTask.Delay((int)(interval * 1000), cancellationToken: token);

                if (_activeTrashInRoom.Count < currentPhase.maxTrashInRoom) SpawnFromPool();
            }
        }

        private void SpawnFromPool()
        {
            if (spawnPoints.Length == 0) return;

            // [Object Pooling] Pull from warehouse instead of allocating new memory
            GameObject trash = _trashPool.Get();

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            trash.transform.position = point.position;
            trash.transform.rotation = point.rotation;

            // [Dependency Injection] Ensure the trash object knows how to return to the pool
            if (trash.TryGetComponent<TrashPickup>(out var pickup))
            {
                pickup.SetPool(_trashPool, trashData);
            }

            _activeTrashInRoom.Add(trash);
        }

        public void ReportTrashRemoved(GameObject trash)
        {
            if (_activeTrashInRoom.Contains(trash)) _activeTrashInRoom.Remove(trash);
        }

        public void EvaluateYeetPressure()
        {
            // [Moral Algorithm] Pressure increases based on customer queue length during disposal
            int queueCount = QueueManager.Instance.CurrentQueueCount;
            if (queueCount > 0)
            {
                var phase = GamePhaseManager.Instance.CurrentPhase;
                float multiplier = (phase != null) ? phase.globalPressureMultiplier : 1f;

                PressureManager.Instance.AddPressure(2.5f * queueCount * multiplier);
            }
        }

        private void OnDestroy() => _cts.Cancel();
    }
}