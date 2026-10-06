using System.Collections.Generic;

namespace Game2Week.Battle.Patterns.Director
{
    /// <summary>전투 동안 유지되는 Director 기록 (EncounterMemory에 저장, 세이브하지 않음).</summary>
    public sealed class DirectorState
    {
        /// <summary>새 패턴(0번)을 충분히 보여 줬는지 — 그 전에는 매 턴 새 패턴을 단독으로 먼저</summary>
        public bool Introduced;
        public readonly List<int> Bag = new();
        public int Last = -1;
        public int Turns;
        public System.Random Random;
    }

    /// <summary>이번 턴의 계획: 주 패턴 + (동시 상한이 2면) 겹칠 두 번째 패턴, 없으면 -1.</summary>
    public readonly struct TurnPlan
    {
        public TurnPlan(int primary, int secondary) { Primary = primary; Secondary = secondary; }
        public int Primary { get; }
        public int Secondary { get; }
    }

    /// <summary>
    /// 턴마다 돌릴 패턴을 고른다 (순수 로직, 5단계).
    /// 0번 = 이 맵의 새 패턴. 소개 전에는 0번 단독, 소개 후에는 셔플 백(연속 금지)으로 순환하고
    /// 동시 상한이 2면 색이 겹쳐도 되는(빨강·파랑 금지) 다른 패턴을 두 번째로 고른다.
    /// </summary>
    public static class DirectorPlanner
    {
        public static TurnPlan PlanTurn(DirectorState state, IReadOnlyList<AttackColor> colors, int maxSimultaneous)
        {
            state.Turns++;
            int count = colors.Count;
            if (count == 0) return new TurnPlan(-1, -1);
            if (!state.Introduced || count == 1)
            {
                state.Last = 0;
                return new TurnPlan(0, -1);
            }

            int primary = Draw(state, count);
            state.Last = primary;
            int secondary = -1;
            if (maxSimultaneous >= 2)
            {
                var options = new List<int>();
                for (int i = 0; i < count; i++)
                    if (i != primary && ColorCombinationRules.CanOverlap(colors[primary], colors[i])) options.Add(i);
                if (options.Count > 0) secondary = options[state.Random.Next(options.Count)];
            }
            return new TurnPlan(primary, secondary);
        }

        /// <summary>셔플 백에서 하나 — 비면 다시 섞고, 직전과 같은 패턴은 피한다.</summary>
        static int Draw(DirectorState state, int count)
        {
            if (state.Bag.Count == 0)
            {
                for (int i = 0; i < count; i++) state.Bag.Add(i);
                for (int i = state.Bag.Count - 1; i > 0; i--)
                {
                    int j = state.Random.Next(i + 1);
                    (state.Bag[i], state.Bag[j]) = (state.Bag[j], state.Bag[i]);
                }
            }
            int pick = 0;
            if (state.Bag[0] == state.Last && state.Bag.Count > 1) pick = 1;
            else if (state.Bag[0] == state.Last)
            {
                // 백에 직전 패턴 하나만 남음 → 새로 섞어 다른 것부터
                state.Bag.Clear();
                for (int i = 0; i < count; i++) if (i != state.Last) state.Bag.Add(i);
                state.Bag.Add(state.Last);
                pick = state.Random.Next(state.Bag.Count - 1);
            }
            int chosen = state.Bag[pick];
            state.Bag.RemoveAt(pick);
            return chosen;
        }
    }
}
