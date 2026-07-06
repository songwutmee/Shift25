using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Gameplay
{
    // [Data-Driven Design] Stores microwave timing and links to narrative dialogue.
    [CreateAssetMenu(fileName = "MicrowaveRequest", menuName = "Shift25/Microwave Request")]
    public class MicrowaveRequestData : ScriptableObject
    {
        [Header("Instruction Setting")]
        public string instructionPhrase; // Simple text for quick reference
        
        public DialogueData instructionDialogue; 

        [Header("Timing Thresholds (Seconds)")]
        public float minAcceptableTime;  
        public float maxAcceptableTime;  

        [Header("Failure Dialogues")]
        public DialogueData tooColdDialogue;
        public DialogueData tooHotDialogue;

        

        // It returns the DialogueData object to be displayed on screen.
        public DialogueData GetDialogue()
        {
            return instructionDialogue;
        }
    }
}