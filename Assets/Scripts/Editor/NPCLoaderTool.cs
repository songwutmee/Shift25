using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal; 
using Shift25.Gameplay;

namespace Shift25.EditorTools
{
    // [Editor Scripting] Advanced tool to clone all logic components from a source NPC to selected models.
    public class NPCLoaderTool : EditorWindow
    {
        private GameObject sourceNPC;

        [MenuItem("Tools/Shift25/NPC Full Setup Syncer")]
        public static void ShowWindow() => GetWindow<NPCLoaderTool>("Full NPC Syncer");

        private void OnGUI()
        {
            GUILayout.Label("Clone All Components", EditorStyles.boldLabel);
            sourceNPC = (GameObject)EditorGUILayout.ObjectField("Source NPC (Template)", sourceNPC, typeof(GameObject), true);

            if (GUILayout.Button("Sync All Components to Selection"))
            {
                SyncAllComponents();
            }
        }

        private void SyncAllComponents()
        {
            if (sourceNPC == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Source NPC first!", "OK");
                return;
            }

            // [Data Structure] Get all components from the source object
            Component[] components = sourceNPC.GetComponents<Component>();

            // [Algorithm] Loop through each selected object in the Hierarchy
            foreach (GameObject target in Selection.gameObjects)
            {
                if (target == sourceNPC) continue;

                Undo.RecordObject(target, "Sync NPC Components");

                foreach (var comp in components)
                {
                    // [Logic Filter] Skip visual-specific components and Transform
                    if (comp is Transform || comp is MeshFilter || comp is MeshRenderer || comp is SkinnedMeshRenderer)
                        continue;

                    CopyAndPasteComponent(comp, target);
                }

                Debug.Log($"[Tool] Deep-synced all logic components to {target.name}");
            }
            
            EditorUtility.DisplayDialog("Success", "All logic components synced!", "Great");
        }

        private void CopyAndPasteComponent(Component sourceComp, GameObject targetGO)
        {
            // [UnityInternal API] Use ComponentUtility to mimic Right-Click Copy/Paste
            ComponentUtility.CopyComponent(sourceComp);
            
            // Check if the target already has this component type
            Component existingComp = targetGO.GetComponent(sourceComp.GetType());

            if (existingComp != null)
            {
                // [Optimization] Overwrite values if component exists
                ComponentUtility.PasteComponentValues(existingComp);
            }
            else
            {
                // [Logic] Add new component if it doesn't exist
                ComponentUtility.PasteComponentAsNew(targetGO);
            }
        }
    }
}