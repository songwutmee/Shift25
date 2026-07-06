using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using Shift25.Managers;
using System.Collections.Generic;

namespace Shift25.Gameplay
{
    // [Singleton Pattern] Factory for NPCs that tracks active customer count.
    public class NPCSpawner : MonoBehaviour
    {
        public static NPCSpawner Instance { get; private set; }

        [Header("NPC Variants")]
        [SerializeField] private List<NPCController> npcPrefabs;

        [Header("Spawn Points")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private List<Transform> browsingPoints;

        // [Object Pooling] Warehouse for recycling different NPC models
        private IObjectPool<NPCController> _npcPool;
        
        // [Data Structure] Counter for Contextual Narrative checking
        private int _activeNPCCount = 0;
        public int ActiveNPCCount => _activeNPCCount; // Public getter for Managers

        private void Awake()
        {
            if (Instance == null) Instance = this;

            // [Factory Pattern] Initializing the heterogeneous pool
            _npcPool = new ObjectPool<NPCController>(
                createFunc: CreateRandomNPC, 
                actionOnGet: (npc) => { npc.gameObject.SetActive(true); _activeNPCCount++; },
                actionOnRelease: (npc) => { npc.gameObject.SetActive(false); _activeNPCCount--; },
                actionOnDestroy: (npc) => Destroy(npc.gameObject),
                defaultCapacity: 5,
                maxSize: 15
            );
        }

        private NPCController CreateRandomNPC()
        {
            if (npcPrefabs == null || npcPrefabs.Count == 0) return null;
            int randomIndex = Random.Range(0, npcPrefabs.Count);
            return Instantiate(npcPrefabs[randomIndex]);
        }

        private void Start() => SpawningLoop().Forget();

        private async UniTaskVoid SpawningLoop()
        {
            await UniTask.Yield();

            while (true)
            {
                var phaseManager = GamePhaseManager.Instance;
                if (phaseManager == null || phaseManager.CurrentPhase == null)
                {
                    await UniTask.Delay(500);
                    continue;
                }

                var settings = phaseManager.CurrentPhase;

                // Capacity Guard: Don't overfill the store
                if (_activeNPCCount >= settings.maxNPCInStore)
                {
                    await UniTask.Delay(1000);
                    continue;
                }

                float delay = Random.Range(settings.minSpawnInterval, settings.maxSpawnInterval);
                await UniTask.Delay((int)(delay * 1000));

                NPCController npc = _npcPool.Get();
                if (npc != null)
                {
                    npc.transform.position = spawnPoint.position;
                    npc.Initialize(exitPoint, _npcPool, browsingPoints);
                }
            }
        }
    }
}