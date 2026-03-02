using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Managers // [Namespace Fix] ให้อยู่บ้านเดียวกับ GamePhaseManager
{
    [CreateAssetMenu(fileName = "PhaseSettings", menuName = "Shift25/Phase Settings")]
    public class PhaseSettings : ScriptableObject
    {
        public int phaseNumber;
        public float durationInSeconds;

        [Header("NPC & Queue Capacity")]
        public int maxNPCInStore; 
        public float minSpawnInterval;
        public float maxSpawnInterval;

        [Header("Item Scanning Data")]
        public int minItemsPerCustomer;
        public int maxItemsPerCustomer;
        public List<ScanItemData> availableItems; 

        [Header("Microwave Data")]
        public List<Shift25.Gameplay.MicrowaveRequestData> availableMicrowaveRequests;

        [Header("Trash System Settings")]
        public float minTrashSpawnInterval = 30f; 
        public float maxTrashSpawnInterval = 60f; 
        public int maxTrashInRoom = 5;            

    }

    
}