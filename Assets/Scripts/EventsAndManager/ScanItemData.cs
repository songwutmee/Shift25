using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScanItem", menuName = "Shift25/Scan Item Data")]
public class ScanItemData : ScriptableObject
{
    public string itemName; // e.g., "Milk", "Bread", "Eggs"

    // List of prefabs sharing the same scan logic
    public List<GameObject> itemPrefabs; 
    
    public float baseScanTime = 0.8f; 

    // Picks a random visual from the list
    public GameObject GetRandomPrefab()
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0) return null;
        return itemPrefabs[Random.Range(0, itemPrefabs.Count)];
    }
}