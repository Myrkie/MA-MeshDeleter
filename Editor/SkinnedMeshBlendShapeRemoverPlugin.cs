using System.Collections.Generic;
using myrkur.dev.ndmf.editor;
using myrkur.dev.ndmf.runtime;
using nadena.dev.ndmf;
using UnityEngine;


[assembly: ExportsPlugin(typeof(SkinnedMeshBlendShapeRemoverPlugin))]

namespace myrkur.dev.ndmf.editor
{
    public class SkinnedMeshBlendShapeRemoverPlugin : Plugin<SkinnedMeshBlendShapeRemoverPlugin>
    {
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Mesh originalMesh;

        public override string QualifiedName => "myrkur.dev.av3-build-framework.delete blend shape vert's";
        public override string DisplayName => "Dissolve Blend shape Vertices";
        private static string _version = "1.0";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).Run("Deleting Vertices", DeleteVerts).Then
                .Run("Remove Skin mesh Remover Component", DestroyComponent);
        }

        void DeleteVerts(BuildContext ctx)
        {
            foreach (var pluginBehavior in ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshBlendShapeRemoverBehavior>())
            {
                if (pluginBehavior == null) continue;

                skinnedMeshRenderer = pluginBehavior.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer == null) continue;

                originalMesh = skinnedMeshRenderer.sharedMesh;
                if (originalMesh == null) continue;

                Mesh modifiedMesh = DeleteVerticesByBlendShape(originalMesh, pluginBehavior.blendShapeName);

                skinnedMeshRenderer.sharedMesh = modifiedMesh;
            }
        }

        private static void DestroyComponent(BuildContext ctx)
        {
            foreach (var behavior in ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshBlendShapeRemoverBehavior>())
            {
                Object.DestroyImmediate(behavior);
            }
        }

        Mesh DeleteVerticesByBlendShape(Mesh originalMesh, string shapeName)
        {
            int shapeIndex = originalMesh.GetBlendShapeIndex(shapeName);
            if (shapeIndex < 0)
            {
                Debug.LogError("Blend shape not found: " + shapeName);
                return originalMesh;
            }

            // store old mesh data
            Vector3[] vertices = originalMesh.vertices;
            Vector3[] normals = originalMesh.normals;
            Vector4[] tangents = originalMesh.tangents;
            Vector2[] uvs = originalMesh.uv;
            BoneWeight[] boneWeights = originalMesh.boneWeights;
            Matrix4x4[] bindposes = originalMesh.bindposes;

            List<int> verticesToDelete = GetBlendShapeAffectedVertices(originalMesh, shapeIndex);
            if (verticesToDelete.Count >= vertices.Length)
            {
                Debug.LogWarning(
                    "All vertices are affected by the blend shape; skipping deletion to prevent an empty mesh.");
                return originalMesh;
            }

            // store new mesh data
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            List<Vector4> newTangents = new List<Vector4>();
            List<Vector2> newUVs = new List<Vector2>();
            List<BoneWeight> newBoneWeights = new List<BoneWeight>();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            int newIndex = 0;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (!verticesToDelete.Contains(i))
                {
                    newVertices.Add(vertices[i]);
                    if (normals.Length > i) newNormals.Add(normals[i]);
                    if (tangents.Length > i) newTangents.Add(tangents[i]);
                    if (uvs.Length > i) newUVs.Add(uvs[i]);
                    if (boneWeights.Length > i) newBoneWeights.Add(boneWeights[i]);

                    indexMap[i] = newIndex;
                    newIndex++;
                }
            }

            // Submesh handling
            List<List<int>> newSubmeshTriangles = new List<List<int>>();
            for (int submesh = 0; submesh < originalMesh.subMeshCount; submesh++)
            {
                int[] submeshTriangles = originalMesh.GetTriangles(submesh);
                List<int> newTriangles = new List<int>();

                for (int i = 0; i < submeshTriangles.Length; i += 3)
                {
                    int v1 = submeshTriangles[i];
                    int v2 = submeshTriangles[i + 1];
                    int v3 = submeshTriangles[i + 2];

                    if (indexMap.ContainsKey(v1) && indexMap.ContainsKey(v2) && indexMap.ContainsKey(v3))
                    {
                        newTriangles.Add(indexMap[v1]);
                        newTriangles.Add(indexMap[v2]);
                        newTriangles.Add(indexMap[v3]);
                    }
                }

                newSubmeshTriangles.Add(newTriangles);
            }

            // Create the new mesh
            var modifiedMesh = new Mesh
            {
                vertices = newVertices.ToArray(),
                normals = newNormals.ToArray(),
                tangents = newTangents.ToArray(),
                uv = newUVs.ToArray(),
                boneWeights = newBoneWeights.ToArray(),
                bindposes = bindposes,
                subMeshCount = newSubmeshTriangles.Count
            };

            for (int i = 0; i < newSubmeshTriangles.Count; i++)
            {
                modifiedMesh.SetTriangles(newSubmeshTriangles[i], i);
            }

            CopyBlendShapes(originalMesh, modifiedMesh, indexMap);

            modifiedMesh.RecalculateNormals();
            modifiedMesh.RecalculateBounds();

            return modifiedMesh;
        }

        void CopyBlendShapes(Mesh originalMesh, Mesh newMesh, Dictionary<int, int> indexMap)
        {
            int blendShapeCount = originalMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                string shapeName = originalMesh.GetBlendShapeName(i);
                int frameCount = originalMesh.GetBlendShapeFrameCount(i);

                for (int j = 0; j < frameCount; j++)
                {
                    float frameWeight = originalMesh.GetBlendShapeFrameWeight(i, j);
                    Vector3[] deltaVertices = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaNormals = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaTangents = new Vector3[originalMesh.vertexCount];

                    originalMesh.GetBlendShapeFrameVertices(i, j, deltaVertices, deltaNormals, deltaTangents);

                    Vector3[] newDeltaVertices = new Vector3[indexMap.Count];
                    Vector3[] newDeltaNormals = new Vector3[indexMap.Count];
                    Vector3[] newDeltaTangents = new Vector3[indexMap.Count];

                    foreach (var kvp in indexMap)
                    {
                        int oldIndex = kvp.Key;
                        int newIndex = kvp.Value;

                        newDeltaVertices[newIndex] = deltaVertices[oldIndex];
                        newDeltaNormals[newIndex] = deltaNormals[oldIndex];
                        newDeltaTangents[newIndex] = deltaTangents[oldIndex];
                    }

                    newMesh.AddBlendShapeFrame(shapeName, frameWeight, newDeltaVertices, newDeltaNormals,
                        newDeltaTangents);
                }
            }
        }

        List<int> GetBlendShapeAffectedVertices(Mesh mesh, int blendShapeIndex)
        {
            int vertexCount = mesh.vertexCount;
            List<int> affectedVertices = new List<int>();

            Vector3[] deltaVertices = new Vector3[vertexCount];
            Vector3[] deltaNormals = new Vector3[vertexCount];
            Vector3[] deltaTangents = new Vector3[vertexCount];

            mesh.GetBlendShapeFrameVertices(blendShapeIndex, 0, deltaVertices, deltaNormals, deltaTangents);

            for (int i = 0; i < vertexCount; i++)
            {
                if (deltaVertices[i] != Vector3.zero || deltaNormals[i] != Vector3.zero ||
                    deltaTangents[i] != Vector3.zero)
                {
                    affectedVertices.Add(i);
                }
            }

            return affectedVertices;
        }
    }
}
