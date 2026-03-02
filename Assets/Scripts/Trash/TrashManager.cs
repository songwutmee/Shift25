using UnityEngine;
using UnityEngine.Pool;
using Shift25.Managers;
using Shift25.Gameplay;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Shift25.Managers
{
    // [Oversees trash generation and handles the Object Pool warehouse.
    public class TrashManager : MonoBehaviour
    {
        public static TrashManager Instance { get; private set; }

        [Header("Configurations")]
        [SerializeField] private TrashData trashData; 
        [SerializeField] private Transform[] spawnPoints;
        
        // Prevents Garbage Collection spikes by recycling objects.
        private IObjectPool<GameObject> _trashPool;
        private List<GameObject> _activeTrashInRoom = new List<GameObject>();
        private CancellationTokenSource _cts;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _cts = new CancellationTokenSource();

            // Initializing the Pool with standard Unity API.
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
            await UniTask.Yield(); 
            while (!token.IsCancellationRequested)
            {
                var settings = GamePhaseManager.Instance.CurrentPhase;
                if (settings == null) { await UniTask.Delay(1000); continue; }

                float interval = Random.Range(settings.minTrashSpawnInterval, settings.maxTrashSpawnInterval);
                await UniTask.Delay((int)(interval * 1000), cancellationToken: token);

                if (_activeTrashInRoom.Count < settings.maxTrashInRoom)
                {
                    SpawnTrash();
                }
            }
        }

        private void SpawnTrash()
        {
            // Behave like a factory: pull from pool instead of Instantiate.
            GameObject trash = _trashPool.Get(); 
            
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            trash.transform.position = point.position;
            trash.transform.rotation = point.rotation;

            // Give the pooled object a reference to return itself.
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
            int queueCount = QueueManager.Instance.CurrentQueueCount;
            if (queueCount > 0)
            {
                // Moral pressure: Yeeting in front of customers.
                PressureManager.Instance.AddPressure(2.5f * queueCount);
            }
        }

        private void OnDestroy() => _cts.Cancel();
    }
}