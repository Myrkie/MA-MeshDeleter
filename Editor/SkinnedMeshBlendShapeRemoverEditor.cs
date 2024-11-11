using System.Collections.Generic;
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
                int selectedIndex = blendShapeNames.IndexOf(behavior.blendShapeName);

                if (selectedIndex == -1)
                {
                    behavior.blendShapeName = blendShapeNames[0];
                    selectedIndex = 0;
                }

                selectedIndex = EditorGUILayout.Popup("Blend Shape Name", selectedIndex, blendShapeNames.ToArray());

                behavior.blendShapeName = blendShapeNames[selectedIndex];
            }
            else
            {
                EditorGUILayout.HelpBox("No blend shapes found in the mesh.", MessageType.Warning);
            }
        }
    }
}
