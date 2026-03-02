using UnityEngine;
using UnityEditor; // [Namespace] สำหรับจัดการ Editor เท่านั้น
using System.Collections.Generic;

namespace Shift25.EditorTools
{
    public class ModelImportAutoConfig : EditorWindow
    {
        [MenuItem("Tools/Shift25/Enable Read-Write for All Models")]
        public static void EnableReadWriteForAllModels()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model");
            int processedCount = 0;

            Debug.Log($"[Tools] Found {guids.Length} models. Starting automation...");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

                if (importer != null && importer.isReadable == false)
                {
                    importer.isReadable = true;
                    
                    importer.SaveAndReimport();
                    processedCount++;
                    Debug.Log($"[Tools] Updated: {path}");
                }
            }

            // แสดงผลสรุป
            EditorUtility.DisplayDialog("Shift25 Automation", 
                $"Process Complete!\nUpdated {processedCount} models to Read/Write enabled.", "OK");
        }
    }
}