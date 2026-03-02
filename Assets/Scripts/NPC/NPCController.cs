using UnityEngine;
using UnityEngine.AI;
using Shift25.Managers;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Pool;
using System.Threading;

namespace Shift25.Gameplay
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCController : MonoBehaviour, IInteractable
    {
        public enum NPCState { Browsing, MovingToQueue, WaitingInQueue, WaitingForFood, Scanning, Leaving }
        
        private NPCState _currentState;
        private NavMeshAgent _agent;
        private Animator _animator;
        
        private Transform _exitPoint;
        private List<ScanItemData> _shoppingCart = new List<ScanItemData>();
        private IObjectPool<NPCController> _pool;
        private List<Transform> _browsingPoints;
        private CancellationTokenSource _cts;

        private MicrowaveRequestData _mySelectedRequest; 
        private bool _needsMicrowave = false;

        public string InteractionPrompt 
        {
            get {
                if (this == null || _currentState == NPCState.Scanning || _currentState == NPCState.Leaving) return "";
                QueueManager.Instance.GetTargetPoint(this, out int index);
                if (index != 0) return ""; 
                
                float dist = Vector3.Distance(transform.position, QueueManager.Instance.GetTargetPoint(this, out _).position);
                if (dist > 1.2f) return "";

                if (_currentState == NPCState.WaitingInQueue || _currentState == NPCState.MovingToQueue) return "Press E to Scan Items";
                if (_currentState == NPCState.WaitingForFood && MicrowaveManager.Instance.CurrentState == MicrowaveManager.MicrowaveState.Done) return "Press E to Give Food";
                return "";
            }
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            
            // Set high quality avoidance to prevent NPCs from walking through each other
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _agent.avoidancePriority = Random.Range(0, 99);
        }

        public void Initialize(Transform exit, IObjectPool<NPCController> pool, List<Transform> browsePts)
        {
            _exitPoint = exit;
            _pool = pool;
            _browsingPoints = browsePts;
            _shoppingCart.Clear();
            _mySelectedRequest = null;
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var currentPhase = GamePhaseManager.Instance.CurrentPhase;
            if (currentPhase != null)
            {
                int count = Random.Range(currentPhase.minItemsPerCustomer, currentPhase.maxItemsPerCustomer + 1);
                for (int i = 0; i < count; i++)
                {
                    if (currentPhase.availableItems.Count > 0)
                        _shoppingCart.Add(currentPhase.availableItems[Random.Range(0, currentPhase.availableItems.Count)]);
                }

                _needsMicrowave = (Random.value < 0.3f);
                if (_needsMicrowave && currentPhase.availableMicrowaveRequests.Count > 0)
                    _mySelectedRequest = currentPhase.availableMicrowaveRequests[Random.Range(0, currentPhase.availableMicrowaveRequests.Count)];
            }

            SwitchState(NPCState.Browsing);
        }

        public void SwitchState(NPCState newState)
        {
            if (this == null || !gameObject.activeInHierarchy) return;
            
            _currentState = newState;
            _agent.isStopped = false;

            switch (_currentState)
            {
                case NPCState.Browsing: 
                    HandleBrowsing(_cts.Token).Forget(); 
                    break;
                case NPCState.MovingToQueue: 
                    QueueManager.Instance.JoinQueue(this); 
                    break;
                case NPCState.Leaving: 
                    _agent.SetDestination(_exitPoint.position);
                    CheckExitDistance(_cts.Token).Forget(); 
                    break;
            }
        }

        private async UniTaskVoid HandleBrowsing(CancellationToken token)
        {
            try {
                int visits = Random.Range(1, 4);
                for (int i = 0; i < visits; i++) {
                    if (token.IsCancellationRequested || this == null) return;
                    
                    Transform target = _browsingPoints[Random.Range(0, _browsingPoints.Count)];
                    _agent.SetDestination(target.position);

                    // Wait until NPC reaches the shelf
                    await UniTask.WaitUntil(() => !_agent.pathPending && _agent.remainingDistance < 0.6f, cancellationToken: token);
                    
                    _agent.isStopped = true; // Stop walking smoothly
                    await UniTask.Delay(Random.Range(2000, 5000), cancellationToken: token);
                    if (this != null) _agent.isStopped = false;
                }
                SwitchState(NPCState.MovingToQueue);
            } catch (System.OperationCanceledException) { }
        }

        public void RefreshQueuePosition()
        {
            if (this == null || !_agent.isOnNavMesh || _currentState == NPCState.Scanning) return;
            
            Transform target = QueueManager.Instance.GetTargetPoint(this, out int index);
            if (target != null)
            {
                _agent.isStopped = false;
                _agent.SetDestination(target.position);
            }
        }

        private void Update()
        {
            if (_animator == null || _agent == null) return;
            // Animation is driven by actual NavMesh velocity to prevent sliding
            float currentSpeed = _agent.velocity.magnitude;
            _animator.SetBool("IsWalking", currentSpeed > 0.1f);
        }

        public void Interact()
        {
            QueueManager.Instance.GetTargetPoint(this, out int index);
            float dist = Vector3.Distance(transform.position, QueueManager.Instance.GetTargetPoint(this, out _).position);
            
            if (index == 0 && dist < 1.2f)
            {
                if (_currentState == NPCState.WaitingInQueue || _currentState == NPCState.MovingToQueue) StartScanning().Forget();
                else if (_currentState == NPCState.WaitingForFood && MicrowaveManager.Instance.CurrentState == MicrowaveManager.MicrowaveState.Done) CompleteHandover();
            }
        }

        private async UniTaskVoid StartScanning()
        {
            _currentState = NPCState.Scanning;
            _agent.isStopped = true;

            bool scanFinished = await ScanManager.Instance.StartScanSession(_shoppingCart);
            
            if (this == null) return;

            if (scanFinished) {
                if (_needsMicrowave && _mySelectedRequest != null) {
                    MicrowaveManager.Instance.AssignPendingRequest(_mySelectedRequest);
                    _currentState = NPCState.WaitingForFood;
                    _agent.isStopped = false;
                } else {
                    QueueManager.Instance.ShiftQueue();
                    SwitchState(NPCState.Leaving);
                }
            }
        }

        private void CompleteHandover()
        {
            float pressure = MicrowaveManager.Instance.GetResultAndReset();
            PressureManager.Instance.AddPressure(pressure);
            QueueManager.Instance.ShiftQueue();
            SwitchState(NPCState.Leaving);
        }

        private async UniTaskVoid CheckExitDistance(CancellationToken token)
        {
            try {
                while (_currentState == NPCState.Leaving) {
                    if (token.IsCancellationRequested || this == null) return;
                    
                    if (!_agent.pathPending && _agent.remainingDistance < 0.8f) {
                        _pool.Release(this);
                        break;
                    }
                    await UniTask.Delay(500, cancellationToken: token);
                }
            } catch (System.OperationCanceledException) { }
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
        }
    }
}