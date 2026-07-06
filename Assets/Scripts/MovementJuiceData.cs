using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Gameplay
{
    [CreateAssetMenu(fileName = "MovementJuice", menuName = "Shift25/Juice Data")]
    public class MovementJuiceData : ScriptableObject
    {
        [Header("Headbob Settings")]
        public float bobSpeed = 10f;
        public float bobAmount = 0.05f;
        
        [Header("Dynamic Tilt")]
        public float tiltAmount = 2f;
        public float tiltSpeed = 5f;

        [Header("Step Settings")]
        public float stepThreshold = 0.5f; // Used to trigger footstep sounds
    }
}