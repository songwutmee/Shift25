using UnityEngine;
using UnityEngine.Pool;
using Shift25.Managers;
using Cysharp.Threading.Tasks;

namespace Shift25.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrashPickup : MonoBehaviour, IInteractable
    {
        private bool _isHeld = false;
        private Rigidbody _rb;
        private Collider _col;
        private IObjectPool<GameObject> _myPool;
        private TrashData _mySettings;
        
        private float _originalSpeed;
        private PlayerController _player;

        public string InteractionPrompt => _isHeld ? "" : "Press E to Pick Up Trash";

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
        }

        public void SetPool(IObjectPool<GameObject> pool, TrashData data)
        {
            _myPool = pool;
            _mySettings = data;
        }

        public void Interact()
        {
            if (!_isHeld) PickUp().Forget();
        }

        private async UniTaskVoid PickUp()
        {
            _isHeld = true;
            _player = FindObjectOfType<PlayerController>();

            if (_player != null)
            {
                // Store the base speed before applying the debuff
                _originalSpeed = _player.moveSpeed;

                // pply weight multiplier. Lower multiplier = Heavier feel.
                // If multiplier is 0.1, speed becomes 0.4 (Extreme crawl)
                _player.moveSpeed *= _mySettings.weightMultiplier;
                
                Debug.Log($"[Weight System] Holding {_mySettings.trashName}. Speed: {_player.moveSpeed}");
            }
            
            // Physics cleanup to prevent player movement interference
            if (_col != null) _col.enabled = false;
            _rb.isKinematic = true;

            // Positioning for FPS perspective
            transform.SetParent(Camera.main.transform);
            transform.localPosition = new Vector3(0.5f, -0.5f, 0.8f);
            transform.localRotation = Quaternion.identity;
            
            await UniTask.Yield();
        }

        public async void YeetIntoVoid()
        {
            if (!_isHeld) return;

            // Wait for door opening animation
            await UniTask.Delay(400); 

            _isHeld = false;
            transform.SetParent(null);
            
            // Reset player speed to normal after disposal
            if (_player != null)
            {
                _player.moveSpeed = _originalSpeed;
                Debug.Log($"[Weight System] Trash disposed. Speed restored to: {_player.moveSpeed}");
            }

            // Launch into the void
            _rb.isKinematic = false;
            if (_col != null) _col.enabled = true;
            _rb.AddForce(Camera.main.transform.forward * 20f, ForceMode.Impulse);

            TrashManager.Instance.ReportTrashRemoved(this.gameObject);
            TrashManager.Instance.EvaluateYeetPressure();
            
            PlayerStateManager.Instance.SwitchState(PlayerStateManager.PlayerState.Roaming);

            ReturnToPoolAfterDelay().Forget();
        }

        private async UniTaskVoid ReturnToPoolAfterDelay()
        {
            await UniTask.Delay(5000);
            if (_myPool != null) _myPool.Release(this.gameObject);
        }
    }
}