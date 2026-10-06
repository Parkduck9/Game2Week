using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Rendering
{
    /// <summary>임시 리소스용 로우폴리 메시 (면마다 정점을 따로 둬서 각진 음영).</summary>
    public static class LowPolyMesh
    {
        /// <summary>정이십면체를 subdivisions번 쪼갠 구. jitter로 표면을 살짝 울퉁불퉁하게.</summary>
        public static Mesh Icosphere(float radius, int subdivisions, float jitter = 0f, int seed = 1)
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            var verts = new List<Vector3>
            {
                new(-1, t, 0), new(1, t, 0), new(-1, -t, 0), new(1, -t, 0),
                new(0, -1, t), new(0, 1, t), new(0, -1, -t), new(0, 1, -t),
                new(t, 0, -1), new(t, 0, 1), new(-t, 0, -1), new(-t, 0, 1),
            };
            for (int i = 0; i < verts.Count; i++) verts[i] = verts[i].normalized;
            var tris = new List<int>
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
            };

            for (int s = 0; s < subdivisions; s++)
            {
                var cache = new Dictionary<long, int>();
                int Mid(int a, int b)
                {
                    long key = a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
                    if (cache.TryGetValue(key, out int index)) return index;
                    verts.Add(((verts[a] + verts[b]) * 0.5f).normalized);
                    return cache[key] = verts.Count - 1;
                }
                var next = new List<int>();
                for (int i = 0; i < tris.Count; i += 3)
                {
                    int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                    int ab = Mid(a, b), bc = Mid(b, c), ca = Mid(c, a);
                    next.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }
                tris = next;
            }

            var rng = new System.Random(seed);
            for (int i = 0; i < verts.Count; i++)
                verts[i] *= radius * (1f + ((float)rng.NextDouble() - 0.5f) * 2f * jitter);

            return Flat(verts, tris, "LowPoly_Icosphere");
        }

        /// <summary>위아래로 뾰족한 팔면체 (보석).</summary>
        public static Mesh Octahedron(float radius, float height)
        {
            var verts = new List<Vector3>
            {
                new(0, height, 0), new(0, -height, 0),
                new(radius, 0, 0), new(0, 0, radius), new(-radius, 0, 0), new(0, 0, -radius),
            };
            var tris = new List<int> { 0, 3, 2, 0, 4, 3, 0, 5, 4, 0, 2, 5, 1, 2, 3, 1, 3, 4, 1, 4, 5, 1, 5, 2 };
            return Flat(verts, tris, "LowPoly_Octahedron");
        }

        /// <summary>밑면이 열린 원뿔 (뿔 장식).</summary>
        public static Mesh Cone(float radius, float height, int segments)
        {
            var verts = new List<Vector3> { new(0, height, 0) };
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                verts.Add(new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius));
            }
            var tris = new List<int>();
            for (int i = 0; i < segments; i++) tris.AddRange(new[] { 0, 1 + (i + 1) % segments, 1 + i });
            return Flat(verts, tris, "LowPoly_Cone");
        }

        static Mesh Flat(List<Vector3> verts, List<int> tris, string name)
        {
            var positions = new Vector3[tris.Count];
            var indices = new int[tris.Count];
            for (int i = 0; i < tris.Count; i++)
            {
                positions[i] = verts[tris[i]];
                indices[i] = i;
            }
            var mesh = new Mesh { name = name, vertices = positions, triangles = indices };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
