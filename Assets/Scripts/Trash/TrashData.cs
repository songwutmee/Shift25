using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift25.Gameplay
{
    [CreateAssetMenu(fileName = "NewTrash", menuName = "Shift25/Trash Data")]
    public class TrashData : ScriptableObject
    {
        public string trashName;
        public GameObject trashPrefab;
        public float weightMultiplier = 0.8f; 
    }
}