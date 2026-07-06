using System.Collections.Generic;
using UnityEngine;
using Shift25.Gameplay;

namespace Shift25.Managers
{
    // [Singleton Pattern] Manages the FIFO queue of customers.
    public class QueueManager : MonoBehaviour
    {
        public static QueueManager Instance { get; private set; }
        [SerializeField] private List<Transform> queuePoints; 
        private List<NPCController> _npcInQueue = new List<NPCController>();

        public int CurrentQueueCount => _npcInQueue.Count;

        private void Awake() => Instance = this;

        public void JoinQueue(NPCController npc)
        {
            if (npc == null) return;
            if (!_npcInQueue.Contains(npc))
            {
                _npcInQueue.Add(npc);
                npc.RefreshQueuePosition();
            }
        }

        public Transform GetTargetPoint(NPCController npc, out int index)
        {
            index = _npcInQueue.IndexOf(npc);
            if (index == -1 || index >= queuePoints.Count) return null;
            
            return queuePoints[index];
        }

        public bool IsFirstInLineAndReady(NPCController npc)
        {
            if (_npcInQueue.Count == 0 || npc == null) return false;
            return _npcInQueue[0] == npc;
        }

        public void ShiftQueue()
        {
            if (_npcInQueue.Count > 0)
            {
                _npcInQueue.RemoveAt(0);
                // [Observer Pattern] Refresh positions for all remaining NPCs
                for (int i = _npcInQueue.Count - 1; i >= 0; i--)
                {
                    if (_npcInQueue[i] != null) 
                        _npcInQueue[i].RefreshQueuePosition();
                    else 
                        _npcInQueue.RemoveAt(i); // Clean up destroyed/null references
                }
            }
        }
    }
}