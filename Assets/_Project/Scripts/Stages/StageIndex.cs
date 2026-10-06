using System;
using System.Collections.Generic;

namespace Game2Week.Stages
{
    /// <summary>stages.json — 게임에서 진행하는 스테이지 순서 (1 → n).</summary>
    [Serializable]
    public sealed class StageIndex
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public List<string> stages = new();
        public StageToolInfo _tool = new();
    }
}
