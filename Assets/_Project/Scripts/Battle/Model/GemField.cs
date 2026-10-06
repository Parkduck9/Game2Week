using System;
using System.Collections.Generic;
using System.Linq;
using Game2Week.Stages;

namespace Game2Week.Battle
{
    /// <summary>
    /// 경기장의 보석 상태. 보석은 "자주 나오지 않게" — 탄막 턴마다 각 위치가 자기 확률로 굴려지고,
    /// 그중 최대 maxPerTurn개만 나타난다. 먹은 보석은 그 전투에서 다시 나오지 않는다.
    /// </summary>
    public sealed class GemField
    {
        readonly List<StageGem> gems;
        readonly int maxPerTurn;
        readonly Random random;
        readonly HashSet<string> collected = new();
        readonly List<string> active = new();

        public GemField(IEnumerable<StageGem> gems, int maxPerTurn, Random random = null)
        {
            this.gems = gems?.ToList() ?? new List<StageGem>();
            this.maxPerTurn = Math.Max(0, maxPerTurn);
            this.random = random ?? new Random();
        }

        public IReadOnlyList<string> Active => active;
        public int CollectedCount => collected.Count;

        /// <summary>새 탄막 턴: 남은 보석을 굴려 이번 턴에 나올 보석 id를 정한다 (이전 턴 보석은 사라짐).</summary>
        public IReadOnlyList<string> RollForTurn()
        {
            active.Clear();
            var candidates = gems
                .Where(g => !collected.Contains(g.id) && random.NextDouble() < g.spawnChance)
                .OrderBy(_ => random.Next())
                .Take(maxPerTurn)
                .Select(g => g.id);
            active.AddRange(candidates);
            return active;
        }

        /// <summary>보석을 먹는다. 이번 턴에 나와 있지 않으면 null.</summary>
        public StageGem Collect(string gemId)
        {
            if (!active.Remove(gemId)) return null;
            collected.Add(gemId);
            return gems.First(g => g.id == gemId);
        }

        /// <summary>탄막 턴 종료 — 남아 있던 보석은 사라진다 (먹은 것으로 치지 않음).</summary>
        public void EndTurn() => active.Clear();
    }
}
