using System.Collections.Generic;
using UnityEngine;

namespace Explorer.Core.MeshGeneration
{
    /// <summary>
    /// Generates icosphere meshes with configurable subdivision levels.
    /// An icosphere has uniformly distributed triangles, ideal for planets.
    /// </summary>
    public static class IcoSphereGenerator
    {
        // Golden ratio for icosahedron construction
        private const float PHI = 1.61803398875f;

        /// <summary>
        /// Creates an icosphere mesh with the specified radius and subdivision level.
        /// </summary>
        /// <param name="radius">Radius of the sphere.</param>
        /// <param name="subdivisions">Number of subdivision iterations (0-5 recommended). 
        /// Vertex count grows exponentially: 12, 42, 162, 642, 2562, 10242.</param>
        /// <returns>A new Mesh instance.</returns>
        public static Mesh Create(float radius, int subdivisions)
        {
            subdivisions = Mathf.Clamp(subdivisions, 0, 6);

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var vertexCache = new Dictionary<long, int>();

            // Create initial icosahedron vertices
            CreateIcosahedronVertices(vertices);

            // Create initial icosahedron faces
            var faces = CreateIcosahedronFaces();

            // Subdivide faces
            for (int i = 0; i < subdivisions; i++)
            {
                var newFaces = new List<TriangleIndices>();
                foreach (var face in faces)
                {
                    int a = GetMiddlePoint(face.V1, face.V2, vertices, vertexCache);
                    int b = GetMiddlePoint(face.V2, face.V3, vertices, vertexCache);
                    int c = GetMiddlePoint(face.V3, face.V1, vertices, vertexCache);

                    newFaces.Add(new TriangleIndices(face.V1, a, c));
                    newFaces.Add(new TriangleIndices(face.V2, b, a));
                    newFaces.Add(new TriangleIndices(face.V3, c, b));
                    newFaces.Add(new TriangleIndices(a, b, c));
                }
                faces = newFaces;
            }

            // Build triangle array
            foreach (var face in faces)
            {
                triangles.Add(face.V1);
                triangles.Add(face.V2);
                triangles.Add(face.V3);
            }

            // Scale vertices to radius and generate normals/UVs
            var normals = new Vector3[vertices.Count];
            var uvs = new Vector2[vertices.Count];
            var scaledVertices = new Vector3[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 normalized = vertices[i].normalized;
                scaledVertices[i] = normalized * radius;
                normals[i] = normalized;
                uvs[i] = CalculateSphericalUV(normalized);
            }

            // Create mesh
            var mesh = new Mesh();
            mesh.name = $"IcoSphere_r{radius}_s{subdivisions}";

            if (scaledVertices.Length > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.vertices = scaledVertices;
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.RecalculateBounds();

            return mesh;
        }

        private static void CreateIcosahedronVertices(List<Vector3> vertices)
        {
            // Icosahedron has 12 vertices defined by 3 orthogonal golden rectangles
            float t = PHI;

            vertices.Add(new Vector3(-1, t, 0).normalized);
            vertices.Add(new Vector3(1, t, 0).normalized);
            vertices.Add(new Vector3(-1, -t, 0).normalized);
            vertices.Add(new Vector3(1, -t, 0).normalized);

            vertices.Add(new Vector3(0, -1, t).normalized);
            vertices.Add(new Vector3(0, 1, t).normalized);
            vertices.Add(new Vector3(0, -1, -t).normalized);
            vertices.Add(new Vector3(0, 1, -t).normalized);

            vertices.Add(new Vector3(t, 0, -1).normalized);
            vertices.Add(new Vector3(t, 0, 1).normalized);
            vertices.Add(new Vector3(-t, 0, -1).normalized);
            vertices.Add(new Vector3(-t, 0, 1).normalized);
        }

        private static List<TriangleIndices> CreateIcosahedronFaces()
        {
            var faces = new List<TriangleIndices>();

            // 5 faces around point 0
            faces.Add(new TriangleIndices(0, 11, 5));
            faces.Add(new TriangleIndices(0, 5, 1));
            faces.Add(new TriangleIndices(0, 1, 7));
            faces.Add(new TriangleIndices(0, 7, 10));
            faces.Add(new TriangleIndices(0, 10, 11));

            // 5 adjacent faces
            faces.Add(new TriangleIndices(1, 5, 9));
            faces.Add(new TriangleIndices(5, 11, 4));
            faces.Add(new TriangleIndices(11, 10, 2));
            faces.Add(new TriangleIndices(10, 7, 6));
            faces.Add(new TriangleIndices(7, 1, 8));

            // 5 faces around point 3
            faces.Add(new TriangleIndices(3, 9, 4));
            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(3, 2, 6));
            faces.Add(new TriangleIndices(3, 6, 8));
            faces.Add(new TriangleIndices(3, 8, 9));

            // 5 adjacent faces
            faces.Add(new TriangleIndices(4, 9, 5));
            faces.Add(new TriangleIndices(2, 4, 11));
            faces.Add(new TriangleIndices(6, 2, 10));
            faces.Add(new TriangleIndices(8, 6, 7));
            faces.Add(new TriangleIndices(9, 8, 1));

            return faces;
        }

        private static int GetMiddlePoint(int p1, int p2, List<Vector3> vertices, Dictionary<long, int> cache)
        {
            // Check cache first (order-independent key)
            long smallerIndex = Mathf.Min(p1, p2);
            long greaterIndex = Mathf.Max(p1, p2);
            long key = (smallerIndex << 32) + greaterIndex;

            if (cache.TryGetValue(key, out int cachedIndex))
            {
                return cachedIndex;
            }

            // Create new vertex at midpoint (normalized to unit sphere)
            Vector3 middle = (vertices[p1] + vertices[p2]) * 0.5f;
            int index = vertices.Count;
            vertices.Add(middle.normalized);

            cache[key] = index;
            return index;
        }

        private static Vector2 CalculateSphericalUV(Vector3 normal)
        {
            // Spherical UV mapping
            float u = 0.5f + Mathf.Atan2(normal.z, normal.x) / (2f * Mathf.PI);
            float v = 0.5f + Mathf.Asin(normal.y) / Mathf.PI;
            return new Vector2(u, v);
        }

        private struct TriangleIndices
        {
            public int V1, V2, V3;

            public TriangleIndices(int v1, int v2, int v3)
            {
                V1 = v1;
                V2 = v2;
                V3 = v3;
            }
        }
    }
}
