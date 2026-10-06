using System;
using Game2Week.Data;

namespace Game2Week.Battle
{
    /// <summary>전투 중 적의 상태. 데이터 에셋은 읽기만 하고 변하는 값은 여기에 둔다.</summary>
    public sealed class EnemyCombatant
    {
        readonly Random random;
        int nextPatternIndex;
        int nextFlavorIndex;
        int nextLineIndex;

        public EnemyCombatant(EnemyData data, Random random = null)
        {
            Data = data ? data : throw new ArgumentNullException(nameof(data));
            this.random = random ?? new Random();
            CurrentHp = data.MaxHp;
        }

        public EnemyData Data { get; }
        public int MaxHp => Data.MaxHp;
        public int CurrentHp { get; private set; }
        public bool IsDefeated => CurrentHp <= 0;
        public int SpareProgress { get; private set; }
        public bool CanBeSpared => SpareProgress >= Data.SpareThreshold;

        /// <summary>실제로 깎인 HP를 돌려준다.</summary>
        public int TakeDamage(int amount)
        {
            int applied = Math.Clamp(amount, 0, CurrentHp);
            CurrentHp -= applied;
            return applied;
        }

        public void AddSpareProgress(int amount) => SpareProgress += Math.Max(0, amount);

        /// <summary>다음 적 턴에 쓸 패턴. 패턴이 없으면 null.</summary>
        public AttackPatternData NextPattern()
        {
            var patterns = Data.AttackPatterns;
            if (patterns.Count == 0) return null;
            if (Data.PatternOrder == PatternOrder.Random) return patterns[random.Next(patterns.Count)];

            var pattern = patterns[nextPatternIndex % patterns.Count];
            nextPatternIndex++;
            return pattern;
        }

        public string NextFlavorText() => Cycle(Data.FlavorTexts, ref nextFlavorIndex);

        public string NextEnemyTurnLine() => Cycle(Data.EnemyTurnLines, ref nextLineIndex);

        static string Cycle(System.Collections.Generic.IReadOnlyList<string> list, ref int index)
        {
            if (list.Count == 0) return string.Empty;
            var value = list[index % list.Count];
            index++;
            return value;
        }
    }
}
