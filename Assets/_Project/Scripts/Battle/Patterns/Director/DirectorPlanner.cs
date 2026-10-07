using System.Collections.Generic;

namespace Game2Week.Battle.Patterns.Director
{
    /// <summary>전투 동안 유지되는 Director 기록 (EncounterMemory에 저장, 세이브하지 않음).</summary>
    public sealed class DirectorState
    {
        /// <summary>새 패턴(0번)을 충분히 보여 줬는지 — 그 전에는 매 턴 새 패턴이 주 공격이고 다른 겹은 소개 뒤에 합류</summary>
        public bool Introduced;
        public readonly List<int> Bag = new();
        public int Last = -1;
        public int Turns;
        /// <summary>지난 턴의 적 체력 단계 (단계가 바뀐 턴을 알기 위해)</summary>
        public int Phase;
        public System.Random Random;
    }

    /// <summary>
    /// 이번 턴의 계획: 겹(레이어) 목록. 0번 = 주 공격, 1번부터 방해·저격 겹 (없으면 -1).
    /// 소개 턴이면 다른 겹은 새 패턴을 소개한 뒤에 합류한다.
    /// </summary>
    public readonly struct TurnPlan
    {
        readonly int[] layers;

        public TurnPlan(int[] layers, bool introTurn)
        {
            this.layers = layers ?? System.Array.Empty<int>();
            IsIntroTurn = introTurn;
        }

        public IReadOnlyList<int> Layers => layers ?? System.Array.Empty<int>();
        public int LayerCount => Layers.Count;
        public int Primary => Layer(0);
        public int Secondary => Layer(1);
        public int Third => Layer(2);
        /// <summary>새 패턴을 처음 소개하는 턴 — 다른 겹이 늦게 합류</summary>
        public bool IsIntroTurn { get; }

        public int Layer(int index) => index < Layers.Count ? Layers[index] : -1;
    }

    /// <summary>
    /// 턴마다 돌릴 패턴을 고른다 (순수 로직, 5단계 → 9단계 겹).
    /// 0번 = 이 맵의 새 패턴. 소개 전에는 0번이 주 공격, 소개 후에는 셔플 백(연속 금지)으로 순환한다.
    /// 겹 상한만큼 색이 서로 겹쳐도 되는(빨강·파랑 금지) 다른 패턴을 더한다.
    /// </summary>
    public static class DirectorPlanner
    {
        public static TurnPlan PlanTurn(DirectorState state, IReadOnlyList<AttackColor> colors, int maxLayers)
        {
            state.Turns++;
            int count = colors.Count;
            if (count == 0) return new TurnPlan(System.Array.Empty<int>(), false);

            bool intro = !state.Introduced && count > 1;
            int primary = !state.Introduced || count == 1 ? 0 : Draw(state, count);
            state.Last = primary;

            var chosen = new List<int> { primary };
            for (int layer = 1; layer < maxLayers; layer++)
            {
                var options = new List<int>();
                for (int i = 0; i < count; i++)
                    if (!chosen.Contains(i) && Compatible(colors, chosen, i)) options.Add(i);
                if (options.Count == 0) break;
                chosen.Add(options[state.Random.Next(options.Count)]);
            }
            return new TurnPlan(chosen.ToArray(), intro);
        }

        /// <summary>이미 고른 모든 겹과 색이 겹쳐도 되는지</summary>
        static bool Compatible(IReadOnlyList<AttackColor> colors, List<int> chosen, int candidate)
        {
            foreach (var c in chosen)
                if (!ColorCombinationRules.CanOverlap(colors[c], colors[candidate])) return false;
            return true;
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
