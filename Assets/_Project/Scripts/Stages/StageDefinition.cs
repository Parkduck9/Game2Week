using System;
using System.Collections.Generic;

namespace Game2Week.Stages
{
    // JSON 직렬화 전용 데이터 (JsonUtility). 필드 이름이 곧 JSON 키이므로 바꾸면 schemaVersion을 올린다.
    // 이 파일들은 맵툴(Stage Editor)만 쓴다 — 손으로 고치지 않는다.

    [Serializable]
    public sealed class StageDefinition
    {
        /// <summary>2 (9-0단계): theme·dialogues 추가. 1은 읽을 때 기본값으로 채워지고 저장하면 2가 된다.</summary>
        public const int CurrentSchemaVersion = 2;
        public const int MinSupportedSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public string id = "stage_001";
        public string name = "새 스테이지";
        public StageGrid grid = new();
        public GridPoint playerStart = new(6, 1);
        public StageEnemy enemy = new();
        public List<StageGem> gems = new();
        public StageGemRules gemRules = new();
        public StageEnemyTurn enemyTurn = new();
        /// <summary>경기장 꾸밈 테마 이름 (11단계 그래픽이 읽음). 비우면 기본.</summary>
        public string theme = StageThemes.Default;
        /// <summary>이 스테이지의 대화 파일 id (10단계 대화 시스템이 읽음). 비우면 적 데이터의 대화 사용.</summary>
        public StageDialogues dialogues = new();
        public StageToolInfo _tool = new();
    }

    public static class StageThemes
    {
        public const string Default = "default";
    }

    /// <summary>상황별 대화 파일 id (Assets/StreamingAssets/Dialogues/&lt;id&gt;.json). 빈 문자열 = 없음.</summary>
    [Serializable]
    public sealed class StageDialogues
    {
        public string intro = string.Empty;
        public string phase2 = string.Empty;
        public string spareReady = string.Empty;
        public string victory = string.Empty;
    }

    [Serializable]
    public sealed class StageGrid
    {
        public int width = 12;
        public int depth = 14;
        public float cellSize = 0.5f;
    }

    [Serializable]
    public struct GridPoint : IEquatable<GridPoint>
    {
        public int x;
        public int z;

        public GridPoint(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public bool Equals(GridPoint other) => x == other.x && z == other.z;
        public override bool Equals(object obj) => obj is GridPoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, z);
        public override string ToString() => $"({x}, {z})";
        public static bool operator ==(GridPoint a, GridPoint b) => a.Equals(b);
        public static bool operator !=(GridPoint a, GridPoint b) => !a.Equals(b);
    }

    [Serializable]
    public sealed class StageEnemy
    {
        /// <summary>EnemyData 에셋 이름 (ContentCatalog에서 찾는다)</summary>
        public string enemyId = "Enemy_Test";
        public GridPoint position = new(6, 12);
    }

    [Serializable]
    public sealed class StageGem
    {
        public string id = "gem_01";
        /// <summary><see cref="GemTypes"/> 중 하나</summary>
        public string type = GemTypes.Heal;
        public GridPoint position;
        /// <summary>탄막 턴마다 이 보석이 나타날 확률 (0~1)</summary>
        public float spawnChance = 0.25f;
    }

    [Serializable]
    public sealed class StageGemRules
    {
        /// <summary>한 탄막 턴에 나올 수 있는 최대 보석 수</summary>
        public int maxPerTurn = 1;
    }

    [Serializable]
    public sealed class StageEnemyTurn
    {
        public float duration = 8f;
        /// <summary>AttackPatternData 에셋 이름. 비우면 적 데이터의 기본 패턴 사용</summary>
        public List<string> patterns = new();
    }

    [Serializable]
    public sealed class StageToolInfo
    {
        public string generatedBy = string.Empty;
        public int toolVersion;
        public string checksum = string.Empty;
    }

    public static class GemTypes
    {
        public const string Heal = "heal";
        public const string Attack = "attack";
        public const string Spare = "spare";

        public static readonly IReadOnlyList<string> All = new[] { Heal, Attack, Spare };

        public static bool IsKnown(string type) => type == Heal || type == Attack || type == Spare;
    }
}
