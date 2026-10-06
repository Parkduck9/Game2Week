using System.Collections.Generic;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using UnityEngine;

namespace Game2Week.Battle.Patterns.Director
{
    /// <summary>
    /// 맵의 패턴 구성(PatternEncounterData)을 턴마다 고르고 돌리는 루트 패턴 (5단계).
    /// - 새 패턴은 첫 턴에 단독. 소개 시간이 채워지기 전에 턴이 끝나면 다음 턴에 다시 먼저.
    /// - 이후 셔플 백, 동시 상한 2인 단계에선 색이 겹쳐도 되는 두 번째 패턴을 늦게 겹친다 (살아 있는 위험이 많으면 더 미룸).
    /// - 하위 패턴에 난이도 배율을 Begin 전에 한 번만 적용. 경고·측정용으로 위험 위치를 모아 준다.
    /// </summary>
    public sealed class PatternDirector : MonoBehaviour, IAttackPattern, IThreatSource
    {
        /// <summary>새 패턴을 이만큼 보여 줘야 "소개됨" (첫 예고 + 첫 발사가 지나갈 시간)</summary>
        public const float IntroduceSeconds = 2.0f;

        [SerializeField] PatternEncounterData encounter;

        readonly List<IAttackPattern> active = new();
        readonly List<GameObject> spawned = new();
        readonly List<ThreatPoint> threatBuffer = new();
        List<AttackPatternData> patterns;
        PatternContext context;
        DirectorState state;
        TurnPlan plan;
        bool secondaryStarted;
        float elapsed;

        public PatternEncounterData Encounter => encounter;
        public TurnPlan Plan => plan;
        public IReadOnlyList<IAttackPattern> ActivePatterns => active;
        public IReadOnlyList<GameObject> ActiveObjects => spawned;
        public DirectorState State => state;
        public float Elapsed => elapsed;

        public void Configure(PatternEncounterData value) => encounter = value;

        public void Begin(PatternContext value)
        {
            End();
            if (!encounter || !encounter.profile) { Debug.LogError($"{name}: 패턴 구성 또는 난이도 없음"); return; }
            context = value;
            patterns = encounter.All();
            state = context.Memory.GetOrCreate<DirectorState>("PatternDirector:" + encounter.name);
            state.Random ??= new System.Random(encounter.seed);

            var colors = new List<AttackColor>();
            foreach (var p in patterns) colors.Add(ColorOf(p));
            plan = DirectorPlanner.PlanTurn(state, colors, encounter.profile.maxSimultaneous);
            elapsed = 0f;
            secondaryStarted = false;
            if (plan.Primary >= 0) Spawn(patterns[plan.Primary]);
        }

        public void Tick(float deltaTime)
        {
            if (context == null || deltaTime <= 0f) return;
            elapsed += deltaTime;
            if (!state.Introduced && plan.Primary == 0 && elapsed >= IntroduceSeconds) state.Introduced = true;

            if (!secondaryStarted && plan.Secondary >= 0 && elapsed >= encounter.profile.secondPatternDelay
                && LiveThreats() < encounter.profile.liveThreatCap)
            {
                secondaryStarted = true;
                Spawn(patterns[plan.Secondary]);
            }
            for (int i = 0; i < active.Count; i++) active[i].Tick(deltaTime);
        }

        public void End()
        {
            foreach (var p in active) p.End();
            foreach (var go in spawned) if (go) Destroy(go);
            active.Clear();
            spawned.Clear();
            context = null;
        }

        public void CollectThreats(List<ThreatPoint> threats)
        {
            foreach (var p in active) if (p is IThreatSource source) source.CollectThreats(threats);
        }

        int LiveThreats()
        {
            threatBuffer.Clear();
            CollectThreats(threatBuffer);
            return threatBuffer.Count;
        }

        void Spawn(AttackPatternData data)
        {
            if (!data || !data.PatternPrefab) return;
            var go = Instantiate(data.PatternPrefab, transform);
            go.name = $"Child_{data.name}";
            if (!go.TryGetComponent(out IAttackPattern pattern)) { Destroy(go); return; }
            if (go.TryGetComponent(out IDirectablePattern directable))
                directable.ApplyDifficulty(encounter.profile.IntervalScale, encounter.profile.speedScale);
            spawned.Add(go);
            active.Add(pattern);
            pattern.Begin(context);
        }

        /// <summary>프리팹의 대표 색 (색 조합 규칙용). 모르면 노랑.</summary>
        public static AttackColor ColorOf(AttackPatternData data) =>
            data && data.PatternPrefab && data.PatternPrefab.TryGetComponent(out IDirectablePattern d) ? d.PatternColor : AttackColor.Yellow;

        void OnDestroy() => End();
    }
}
