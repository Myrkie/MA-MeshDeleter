using System.Collections.Generic;
using myrkur.dev.ndmf.runtime;
using UnityEditor;
using UnityEngine;

namespace myrkur.dev.ndmf.editor
{
    [CustomEditor(typeof(SkinnedMeshBlendShapeRemoverBehavior))]
    public class SkinnedMeshBlendShapeRemoverEditor : Editor
    {
        private SkinnedMeshBlendShapeRemoverBehavior behavior;
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Mesh originalMesh;

        private Vector2 scrollPosition;

        private bool isDropdownOpen;

        public override void OnInspectorGUI()
        {
            behavior = (SkinnedMeshBlendShapeRemoverBehavior)target;
            
            skinnedMeshRenderer = behavior.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                originalMesh = skinnedMeshRenderer.sharedMesh;
            }

            PopulateBlendShapeDropdown();
        }
        
        void PopulateBlendShapeDropdown()
        {
            if (originalMesh == null) return;

            List<string> blendShapeNames = new List<string>();

            int blendShapeCount = originalMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                string shapeName = originalMesh.GetBlendShapeName(i);
                blendShapeNames.Add(shapeName);
            }

            if (blendShapeNames.Count > 0)
            {
                EditorGUILayout.LabelField("Blend Shape Name", EditorStyles.boldLabel);
                if (GUILayout.Button(behavior.blendShapeName, EditorStyles.popup))
                {
                    isDropdownOpen = !isDropdownOpen;
                }

                EditorGUILayout.Separator();
                if (isDropdownOpen)
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));
                    foreach (var shapeName in blendShapeNames)
                    {
                        if (GUILayout.Button(shapeName))
                        {
                            behavior.blendShapeName = shapeName;
                            isDropdownOpen = false;
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No blend shapes found in the mesh.", MessageType.Warning);
            }
        }
    }
}
