using UnityEngine;

namespace Game2Week.Stages
{
    /// <summary>격자 좌표 ↔ 경기장 로컬 좌표. 경기장 중심이 원점, +Z가 적 쪽(셀 z가 커지는 방향).</summary>
    public static class StageGeometry
    {
        public static Vector2 ArenaSize(StageGrid grid) => new(grid.width * grid.cellSize, grid.depth * grid.cellSize);

        public static bool InBounds(StageGrid grid, GridPoint cell) =>
            cell.x >= 0 && cell.z >= 0 && cell.x < grid.width && cell.z < grid.depth;

        public static Vector3 CellToLocal(StageGrid grid, GridPoint cell)
        {
            var size = ArenaSize(grid);
            return new Vector3(
                (cell.x + 0.5f) * grid.cellSize - size.x * 0.5f,
                0f,
                (cell.z + 0.5f) * grid.cellSize - size.y * 0.5f);
        }

        public static GridPoint LocalToCell(StageGrid grid, Vector3 local)
        {
            var size = ArenaSize(grid);
            return new GridPoint(
                Mathf.FloorToInt((local.x + size.x * 0.5f) / grid.cellSize),
                Mathf.FloorToInt((local.z + size.y * 0.5f) / grid.cellSize));
        }

        /// <summary>두 셀 사이 거리 (셀 단위, 유클리드)</summary>
        public static float CellDistance(GridPoint a, GridPoint b) => Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));
    }
}
