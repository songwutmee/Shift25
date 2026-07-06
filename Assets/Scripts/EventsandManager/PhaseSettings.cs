using System.Collections.Generic;
using UnityEngine;
using Shift25.Gameplay;

namespace Shift25.Managers
{
    [CreateAssetMenu(fileName = "PhaseSettings", menuName = "Shift25/Phase Settings")]
    public class PhaseSettings : ScriptableObject
    {
        public int phaseNumber;
        public float durationInSeconds;

        [Header("Feature Toggles")]
        public bool enableMicrowave = false;
        public bool enableTrash = false;
        public bool enableCram = false;

        [Header("Difficulty Scaling")]
        public float globalPressureMultiplier = 1.0f; 

        [Header("Narrative - Story Beats")]
        public DialogueData phaseStartDialogue; // Plays once at phase start

        [Header("Narrative - Random Ambient")]
        // [Data Structure] Pool of dialogues that can trigger randomly during this phase
        public List<DialogueData> randomPhaseDialogues; 

        [Header("NPC & Workload")]
        public int maxNPCInStore; 
        public int minItemsPerCustomer;
        public int maxItemsPerCustomer;
        public float minSpawnInterval;
        public float maxSpawnInterval;
        public List<ScanItemData> availableItems; 
        public List<MicrowaveRequestData> availableMicrowaveRequests;

        [Header("Trash & Cram")]
        public int maxTrashInRoom = 5;
        public float minTrashSpawnInterval = 15f; 
        public float maxTrashSpawnInterval = 30f; 
        public float minShelfDepletionInterval = 30f;
        public float maxShelfDepletionInterval = 60f;
    }
}