using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Shift25.Managers
{
    public class ScanManager : MonoBehaviour
    {
        public static ScanManager Instance { get; private set; }
        [SerializeField] private CinemachineVirtualCamera scanCamera;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private float maxDistance = 0.8f;

        private List<GameObject> _activeItems = new List<GameObject>();
        private int _scannedInSession, _totalInSession;
        private bool _active = false;
        private CancellationTokenSource _cts;

        private void Awake() => Instance = this;

        public async UniTask<bool> StartScanSession(List<ScanItemData> items)
        {
            if (_active || items == null || items.Count == 0) return true;
            _active = true; _scannedInSession = 0; _totalInSession = items.Count;
            _cts = new CancellationTokenSource();

            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Interacting);
            scanCamera.Priority = 20;
            UIManager.Instance.SetUIMode(true);

            for (int i = 0; i < items.Count; i++) {
                SpawnItem(items[i]);
                await UniTask.Delay(150, cancellationToken: _cts.Token);
            }

            MonitorBoundaries(_cts.Token).Forget();
            await UniTask.WaitUntil(() => _scannedInSession >= _totalInSession || _activeItems.Count == 0);
            
            await UniTask.Delay(300);
            EndSession();
            return true;
        }

        private void SpawnItem(ScanItemData data) {
            GameObject prefab = data.GetRandomPrefab();
            if (prefab == null) return;
            var item = Instantiate(prefab, spawnRoot.position + Random.insideUnitSphere * 0.1f, Quaternion.identity);
            _activeItems.Add(item);
            if (item.TryGetComponent<Rigidbody>(out var rb)) {
                rb.isKinematic = false; rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            if (item.TryGetComponent<Shift25.Gameplay.ScannableItem>(out var s)) s.Initialize(data);
        }

        private async UniTaskVoid MonitorBoundaries(CancellationToken token) {
            while (_active && !token.IsCancellationRequested) {
                for (int i = _activeItems.Count - 1; i >= 0; i--) {
                    if (_activeItems[i] == null) continue;
                    if (Vector3.Distance(_activeItems[i].transform.position, spawnRoot.position) > maxDistance) {
                        _activeItems[i].transform.position = spawnRoot.position + Vector3.up * 0.2f;
                        if (_activeItems[i].TryGetComponent<Rigidbody>(out var rb)) rb.velocity = Vector3.zero;
                    }
                }
                await UniTask.Delay(200, cancellationToken: token);
            }
        }

        public void ReportItemScanned(GameObject obj) {
            _scannedInSession++; _activeItems.Remove(obj);
            AudioManager.Instance.PlayScanSFX();
        }

        private void EndSession() {
            _active = false; _cts?.Cancel();
            foreach (var item in _activeItems) if (item != null) Destroy(item);
            AudioManager.Instance.PlayScanSFX();
            _activeItems.Clear();
            scanCamera.Priority = 0;
            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Roaming);
            UIManager.Instance.SetUIMode(false);
        }
    }
}