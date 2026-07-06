using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Gameplay
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Shift25/Narrative/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public enum SpeakerType { Customer, Manager, System, Self }
        
        [Header("Identity")]
        public SpeakerType speaker;
        public string speakerName;
        
        [Header("Content")]
        [TextArea(3, 10)]
        public string[] textVariants; // Multiple ways to say the same thing
        public Color textColor = Color.white;

        // Returns a random variation of this dialogue
        public string GetRandomLine()
        {
            if (textVariants.Length == 0) return "";
            return textVariants[Random.Range(0, textVariants.Length)];
        }
    }
}
