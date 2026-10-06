using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 3D 경기장 (= 전투 박스). 스테이지의 격자 크기대로 바닥·벽·격자선을 만든다.
    /// 경기장 중심이 이 오브젝트의 위치이고, +Z가 적 쪽.
    /// </summary>
    public sealed class BattleArena : MonoBehaviour
    {
        [SerializeField] Material floorMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material gridLineMaterial;
        [SerializeField, Min(0.05f)] float wallHeight = 0.3f;
        [SerializeField, Min(0.02f)] float wallThickness = 0.12f;
        [SerializeField] bool showGridLines = true;

        Transform built;
        readonly Renderer[] boundaries = new Renderer[4];

        public StageGrid Grid { get; private set; }
        public Vector2 Size { get; private set; }

        public void Build(StageGrid grid)
        {
            Grid = grid;
            Size = StageGeometry.ArenaSize(grid);
            if (built) Destroy(built.gameObject);
            built = new GameObject("Generated").transform;
            built.SetParent(transform, false);

            Box("Floor", new Vector3(0, -0.05f, 0), new Vector3(Size.x, 0.1f, Size.y), floorMaterial);

            float halfW = Size.x * 0.5f, halfD = Size.y * 0.5f, t = wallThickness, h = wallHeight;
            boundaries[0] = Box("Wall_Near", new Vector3(0, h * 0.5f, -halfD - t * 0.5f), new Vector3(Size.x + t * 2, h, t), wallMaterial);
            boundaries[1] = Box("Wall_Far", new Vector3(0, h * 0.5f, halfD + t * 0.5f), new Vector3(Size.x + t * 2, h, t), wallMaterial);
            boundaries[2] = Box("Wall_Left", new Vector3(-halfW - t * 0.5f, h * 0.5f, 0), new Vector3(t, h, Size.y), wallMaterial);
            boundaries[3] = Box("Wall_Right", new Vector3(halfW + t * 0.5f, h * 0.5f, 0), new Vector3(t, h, Size.y), wallMaterial);

            if (!showGridLines) return;
            const float line = 0.015f, y = 0.002f;
            for (int x = 1; x < grid.width; x++)
                Box($"GridX_{x}", new Vector3(-halfW + x * grid.cellSize, y, 0), new Vector3(line, 0.002f, Size.y), gridLineMaterial);
            for (int z = 1; z < grid.depth; z++)
                Box($"GridZ_{z}", new Vector3(0, y, -halfD + z * grid.cellSize), new Vector3(Size.x, 0.002f, line), gridLineMaterial);
        }

        public Vector3 CellToWorld(GridPoint cell) => transform.TransformPoint(StageGeometry.CellToLocal(Grid, cell));

        /// <summary>반지름 radius인 물체가 벽 안에 있도록 위치를 제한한다 (높이는 그대로).</summary>
        public Vector3 ClampToArena(Vector3 world, float radius)
        {
            var local = transform.InverseTransformPoint(world);
            float maxX = Mathf.Max(0f, Size.x * 0.5f - radius), maxZ = Mathf.Max(0f, Size.y * 0.5f - radius);
            local.x = Mathf.Clamp(local.x, -maxX, maxX);
            local.z = Mathf.Clamp(local.z, -maxZ, maxZ);
            return transform.TransformPoint(local);
        }

        public void UpdateBoundaryVisibility(Vector3? cameraPosition)
        {
            var local = cameraPosition.HasValue ? transform.InverseTransformPoint(cameraPosition.Value) : Vector3.zero;
            bool[] visible = { local.z >= -Size.y * .5f, local.z <= Size.y * .5f, local.x >= -Size.x * .5f, local.x <= Size.x * .5f };
            for (int i = 0; i < boundaries.Length; i++) if (boundaries[i]) boundaries[i].enabled = !cameraPosition.HasValue || visible[i];
        }
        Renderer Box(string name, Vector3 localPos, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(built, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<MeshRenderer>();
            if (material) renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return renderer;
        }
    }
}
