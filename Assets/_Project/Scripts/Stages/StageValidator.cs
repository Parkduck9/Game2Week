using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Game2Week.Stages
{
    public enum IssueSeverity
    {
        Warning,
        Error,
    }

    public readonly struct StageIssue
    {
        public StageIssue(IssueSeverity severity, string message, GridPoint? cell = null)
        {
            Severity = severity;
            Message = message;
            Cell = cell;
        }

        public IssueSeverity Severity { get; }
        public string Message { get; }
        /// <summary>문제가 있는 칸 (맵툴에서 강조 표시)</summary>
        public GridPoint? Cell { get; }

        public override string ToString() => $"[{Severity}] {Message}" + (Cell is { } c ? $" {c}" : string.Empty);
    }

    /// <summary>스테이지가 참조하는 에셋 이름이 실제로 있는지 확인하는 창구 (런타임: ContentCatalog).</summary>
    public interface IStageCatalog
    {
        bool HasEnemy(string enemyId);
        bool HasPattern(string patternId);
    }

    public static class StageLimits
    {
        public const int MinCells = 4;
        public const int MaxCells = 40;
        public const float MinCellSize = 0.25f;
        public const float MaxCellSize = 2f;
        public const float MinPlayerEnemyDistance = 4f;
        /// <summary>시작점-적 권장 최소 거리 (m) — 이동 3.4m/s 기준 직선 약 4.4초</summary>
        public const float RecommendedPlayerEnemyMeters = 15f;
        public const float MinTurnDuration = 2f;
        public const float MaxTurnDuration = 60f;
    }

    /// <summary>맵툴 저장 전 · 게임 로드 시 둘 다 사용하는 검증.</summary>
    public static class StageValidator
    {
        static readonly Regex IdPattern = new("^[a-z0-9_]+$");

        public static bool IsValidId(string id) => !string.IsNullOrEmpty(id) && IdPattern.IsMatch(id);

        public static bool HasErrors(IEnumerable<StageIssue> issues)
        {
            foreach (var issue in issues)
                if (issue.Severity == IssueSeverity.Error) return true;
            return false;
        }

        public static List<StageIssue> Validate(StageDefinition stage, IStageCatalog catalog = null)
        {
            var issues = new List<StageIssue>();
            void Error(string message, GridPoint? cell = null) => issues.Add(new StageIssue(IssueSeverity.Error, message, cell));
            void Warn(string message, GridPoint? cell = null) => issues.Add(new StageIssue(IssueSeverity.Warning, message, cell));

            if (stage == null)
            {
                Error("스테이지 데이터가 없음");
                return issues;
            }

            if (stage.schemaVersion < StageDefinition.MinSupportedSchemaVersion || stage.schemaVersion > StageDefinition.CurrentSchemaVersion)
                Error($"지원하지 않는 schemaVersion: {stage.schemaVersion}");
            if (!IsValidId(stage.id)) Error($"스테이지 id는 영문 소문자·숫자·_ 만 가능: \"{stage.id}\"");
            if (string.IsNullOrWhiteSpace(stage.name)) Warn("스테이지 이름이 비어 있음");

            var grid = stage.grid;
            if (grid == null)
            {
                Error("grid가 없음");
                return issues;
            }
            if (grid.width < StageLimits.MinCells || grid.width > StageLimits.MaxCells ||
                grid.depth < StageLimits.MinCells || grid.depth > StageLimits.MaxCells)
                Error($"맵 크기는 {StageLimits.MinCells}~{StageLimits.MaxCells}칸: {grid.width} × {grid.depth}");
            if (grid.cellSize < StageLimits.MinCellSize || grid.cellSize > StageLimits.MaxCellSize)
                Error($"셀 크기는 {StageLimits.MinCellSize}~{StageLimits.MaxCellSize}m: {grid.cellSize}");

            var occupied = new Dictionary<GridPoint, string>();
            void Occupy(GridPoint cell, string what)
            {
                if (!StageGeometry.InBounds(grid, cell)) Error($"{what}이(가) 경기장 밖에 있음", cell);
                else if (occupied.TryGetValue(cell, out var other)) Error($"{what}이(가) {other}와(과) 같은 칸에 있음", cell);
                else occupied[cell] = what;
            }

            Occupy(stage.playerStart, "주인공 시작점");

            if (stage.enemy == null) Error("적이 없음");
            else
            {
                Occupy(stage.enemy.position, "적");
                if (string.IsNullOrEmpty(stage.enemy.enemyId)) Error("적 종류(enemyId)가 비어 있음", stage.enemy.position);
                else if (catalog != null && !catalog.HasEnemy(stage.enemy.enemyId)) Error($"없는 적 데이터: {stage.enemy.enemyId}", stage.enemy.position);

                if (StageGeometry.CellDistance(stage.playerStart, stage.enemy.position) < StageLimits.MinPlayerEnemyDistance)
                    Error($"주인공 시작점과 적은 {StageLimits.MinPlayerEnemyDistance}칸 이상 떨어져야 함", stage.enemy.position);
                else
                {
                    // 9단계: 적에게 닿는 시간이 너무 짧으면 패턴을 못 보고 턴이 끝난다
                    float meters = StageGeometry.CellDistance(stage.playerStart, stage.enemy.position) * grid.cellSize;
                    if (meters < StageLimits.RecommendedPlayerEnemyMeters)
                        Warn($"주인공 시작점과 적 거리 {meters:0.#}m — {StageLimits.RecommendedPlayerEnemyMeters}m 이상 권장 (너무 빨리 닿음)", stage.enemy.position);
                }
            }

            var gemIds = new HashSet<string>();
            foreach (var gem in stage.gems ?? new List<StageGem>())
            {
                var label = $"보석 {gem.id}";
                if (string.IsNullOrEmpty(gem.id)) Error("보석 id가 비어 있음", gem.position);
                else if (!gemIds.Add(gem.id)) Error($"보석 id 중복: {gem.id}", gem.position);
                if (!GemTypes.IsKnown(gem.type)) Error($"{label}: 알 수 없는 종류 \"{gem.type}\"", gem.position);
                if (gem.spawnChance < 0f || gem.spawnChance > 1f) Error($"{label}: 등장 확률은 0~1", gem.position);
                Occupy(gem.position, label);
            }

            if (stage.gemRules == null || stage.gemRules.maxPerTurn < 0) Error("한 턴 최대 보석 수는 0 이상");

            var turn = stage.enemyTurn;
            if (turn == null) Error("enemyTurn이 없음");
            else
            {
                if (turn.duration < StageLimits.MinTurnDuration || turn.duration > StageLimits.MaxTurnDuration)
                    Error($"탄막 턴 시간은 {StageLimits.MinTurnDuration}~{StageLimits.MaxTurnDuration}초: {turn.duration}");
                if (turn.patterns == null || turn.patterns.Count == 0) Warn("패턴 미지정 — 적 데이터의 기본 패턴 사용");
                else if (catalog != null)
                    foreach (var pattern in turn.patterns)
                        if (!catalog.HasPattern(pattern)) Error($"없는 패턴: {pattern}");
            }

            return issues;
        }
    }
}
