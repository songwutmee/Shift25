using UnityEngine;
using UnityEngine.AI;
using Shift25.Managers;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Pool;
using System.Threading;

namespace Shift25.Gameplay
{
    // [State Pattern] Handles NPC behavior, movement, and interaction rules.
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class NPCController : MonoBehaviour, IInteractable
    {
        public enum NPCState { Idle, Browsing, MovingToQueue, WaitingInQueue, WaitingForFood, Scanning, Leaving }
        private NPCState _currentState = NPCState.Idle;

        private NavMeshAgent _agent;
        private Animator _animator;
        private CancellationTokenSource _cts;

        [Header("Narrative Assets")]
        [SerializeField] private DialogueData waitComplaint;
        [SerializeField] private DialogueData scanComplaint;

        private Transform _exitPoint;
        private IObjectPool<NPCController> _pool;
        private List<Transform> _browsingPoints;
        private List<ScanItemData> _shoppingCart = new List<ScanItemData>();
        
        private MicrowaveRequestData _myRequest;
        private float _queueTimer = 0f;

        public string InteractionPrompt {
            get {
                if (this == null || _currentState == NPCState.Scanning || _currentState == NPCState.Leaving) return "";
                Transform target = QueueManager.Instance.GetTargetPoint(this, out int idx);
                if (target == null || idx != 0) return ""; 
                float dist = Vector3.Distance(transform.position, target.position);
                return (dist < 1.2f) ? "Press E to Interact" : "";
            }
        }

        private void Awake() { 
            _agent = GetComponent<NavMeshAgent>(); 
            _animator = GetComponent<Animator>(); 
        }

        public void Initialize(Transform exit, IObjectPool<NPCController> pool, List<Transform> browsePts)
        {
            _exitPoint = exit; _pool = pool; _browsingPoints = browsePts;
            _shoppingCart.Clear(); _queueTimer = 0f;
            _cts?.Cancel(); _cts = new CancellationTokenSource();

            PopulateCart();
            
            // [Critical Fix] Snap to NavMesh before starting any movement to prevent errors
            SnapToNavMesh();
            SwitchState(NPCState.Browsing);
        }

        private void SnapToNavMesh()
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                _agent.enabled = false; // Reset agent state
                _agent.enabled = true;
            }
        }

        public void SwitchState(NPCState newState)
        {
            _currentState = newState;
            if (this == null || !gameObject.activeInHierarchy || !_agent.enabled || !_agent.isOnNavMesh) return;

            switch (_currentState) {
                case NPCState.Browsing: HandleBrowsing(_cts.Token).Forget(); break;
                case NPCState.MovingToQueue: QueueManager.Instance.JoinQueue(this); break;
                case NPCState.Leaving: 
                    _agent.isStopped = false;
                    _agent.SetDestination(_exitPoint.position);
                    CheckExitDistance(_cts.Token).Forget(); 
                    break;
            }
        }

        private async UniTaskVoid HandleBrowsing(CancellationToken token)
        {
            try {
                int visits = Random.Range(1, 3);
                for (int i = 0; i < visits; i++) {
                    if (token.IsCancellationRequested || this == null || !_agent.isOnNavMesh) return;
                    
                    _agent.SetDestination(_browsingPoints[Random.Range(0, _browsingPoints.Count)].position);
                    
                    // Safety check during movement
                    await UniTask.WaitUntil(() => this != null && _agent.enabled && _agent.isOnNavMesh && !_agent.pathPending && _agent.remainingDistance < 0.6f, cancellationToken: token);
                    
                    _agent.isStopped = true;
                    await UniTask.Delay(Random.Range(2000, 4000), cancellationToken: token);
                    if(this != null) _agent.isStopped = false;
                }
                SwitchState(NPCState.MovingToQueue);
            } catch { }
        }

        public void Interact()
        {
            Transform targetPoint = QueueManager.Instance.GetTargetPoint(this, out int index);
            if (targetPoint == null) return;
            if (index == 0 && Vector3.Distance(transform.position, targetPoint.position) < 1.2f)
            {
                if (_currentState == NPCState.WaitingInQueue || _currentState == NPCState.MovingToQueue) StartScanning().Forget();
                else if (_currentState == NPCState.WaitingForFood && MicrowaveManager.Instance.CurrentState == MicrowaveManager.MicrowaveState.Done) CompleteHandover();
            }
        }

        private async UniTaskVoid StartScanning()
        {
            _currentState = NPCState.Scanning; _agent.isStopped = true;
            bool success = await ScanManager.Instance.StartScanSession(_shoppingCart);
            if (this == null) return;
            if (success) {
                if (_myRequest != null) {
                    MicrowaveManager.Instance.AssignPendingRequest(_myRequest);
                    NarrativeManager.Instance.DisplayMessage(_myRequest.GetDialogue()).Forget();
                    _currentState = NPCState.WaitingForFood; _agent.isStopped = false;
                } else {
                    QueueManager.Instance.ShiftQueue(); SwitchState(NPCState.Leaving);
                }
            }
        }

        private void CompleteHandover()
        {
            float p = MicrowaveManager.Instance.GetResultAndReset();
            PressureManager.Instance.AddPressure(p);
            QueueManager.Instance.ShiftQueue(); SwitchState(NPCState.Leaving);
        }

        private void Update()
        {
            if (_animator != null && _agent != null && _agent.enabled && _agent.isOnNavMesh)
                _animator.SetBool("IsWalking", _agent.velocity.magnitude > 0.1f);

            if (_currentState == NPCState.WaitingInQueue && _agent.isOnNavMesh && _agent.remainingDistance < 0.6f) {
                _queueTimer += Time.deltaTime;
                if (_queueTimer > 15f) {
                    _queueTimer = 0f;
                    PressureManager.Instance.AddPressure(1f);
                    if (waitComplaint != null) NarrativeManager.Instance.DisplayMessage(waitComplaint).Forget();
                }
            }
        }

        public void RefreshQueuePosition()
        {
            if (this == null || !_agent.enabled || !_agent.isOnNavMesh || _currentState == NPCState.Scanning) return;
            Transform target = QueueManager.Instance.GetTargetPoint(this, out _);
            if (target != null) _agent.SetDestination(target.position);
        }

        private void PopulateCart() {
            var phase = GamePhaseManager.Instance.CurrentPhase;
            int count = Mathf.Max(1, Random.Range(phase.minItemsPerCustomer, phase.maxItemsPerCustomer + 1));
            for (int i = 0; i < count; i++)
                _shoppingCart.Add(phase.availableItems[Random.Range(0, phase.availableItems.Count)]);
            if (phase.enableMicrowave && Random.value < 0.35f && phase.availableMicrowaveRequests.Count > 0)
                _myRequest = phase.availableMicrowaveRequests[Random.Range(0, phase.availableMicrowaveRequests.Count)];
        }

        private async UniTaskVoid CheckExitDistance(CancellationToken token)
        {
            try {
                while (_currentState == NPCState.Leaving) {
                    if (token.IsCancellationRequested || this == null) return;
                    if (_agent.enabled && _agent.isOnNavMesh && !_agent.pathPending && _agent.remainingDistance < 0.8f) {
                        _pool.Release(this); break;
                    }
                    await UniTask.Delay(500, cancellationToken: token);
                }
            } catch { }
        }

        private void OnDisable() { _cts?.Cancel(); if(_agent != null) _agent.enabled = false; }
    }
}