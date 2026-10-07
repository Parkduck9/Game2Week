using System.Collections.Generic;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using UnityEngine;

namespace Game2Week.Battle.Patterns.Director
{
    /// <summary>
    /// 맵의 패턴 구성(PatternEncounterData) + 적 고유 기술을 턴마다 고르고 돌리는 루트 패턴 (5단계 → 9단계).
    /// - 후보 = 맵 패턴(0번 = 새 패턴) + 적 고유 기술 + (체력 단계 1 이상) 단계 기술.
    /// - 한 턴에 겹(주 공격 + 방해 + 저격)을 최대 3개까지. 빨강·파랑은 같은 턴에 겹치지 않는다.
    /// - 새 패턴은 소개 턴에 주 공격으로 먼저 혼자 나오고, 소개 시간이 지나면 다른 겹이 합류한다.
    /// - 적 체력 단계마다 겹 수·간격·탄속이 바뀐다 (DifficultyProfile.phases). 단계가 오른 턴에 알림.
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
        bool[] layerStarted;
        float elapsed, intervalScale, speedScale;

        public PatternEncounterData Encounter => encounter;
        public TurnPlan Plan => plan;
        /// <summary>이번 턴 후보 패턴 (계획의 번호가 가리키는 목록)</summary>
        public IReadOnlyList<AttackPatternData> CandidatePatterns => patterns;
        public IReadOnlyList<IAttackPattern> ActivePatterns => active;
        public IReadOnlyList<GameObject> ActiveObjects => spawned;
        public DirectorState State => state;
        public float Elapsed => elapsed;
        /// <summary>이번 턴의 적 체력 단계 (0 = 기본)</summary>
        public int Phase { get; private set; }

        public void Configure(PatternEncounterData value) => encounter = value;

        public void Begin(PatternContext value)
        {
            End();
            if (!encounter || !encounter.profile) { Debug.LogError($"{name}: 패턴 구성 또는 난이도 없음"); return; }
            context = value;
            var profile = encounter.profile;
            state = context.Memory.GetOrCreate<DirectorState>("PatternDirector:" + encounter.name);
            state.Random ??= new System.Random(encounter.seed);

            Phase = profile.PhaseFor(context.EnemyInfo.HealthRatio);
            if (Phase > state.Phase) context.Feedback.RaiseEnemyPhaseChanged(Phase);
            state.Phase = Mathf.Max(state.Phase, Phase);
            (intervalScale, speedScale) = profile.ScalesFor(Phase);

            patterns = BuildCandidates(encounter, context.EnemyInfo, Phase);
            var colors = new List<AttackColor>();
            foreach (var p in patterns) colors.Add(ColorOf(p));
            plan = DirectorPlanner.PlanTurn(state, colors, profile.LayersFor(Phase));
            elapsed = 0f;
            layerStarted = new bool[plan.LayerCount];
            if (plan.Primary >= 0) { layerStarted[0] = true; Spawn(patterns[plan.Primary]); }
        }

        /// <summary>후보 = 맵 패턴(0번 = 새 패턴) + 적 고유 기술 + (단계 1 이상) 단계 기술, 중복 제거</summary>
        public static List<AttackPatternData> BuildCandidates(PatternEncounterData encounter, EnemyPatternInfo enemy, int phase)
        {
            var all = encounter.All();
            void Add(IReadOnlyList<AttackPatternData> list) { foreach (var p in list) if (p && !all.Contains(p)) all.Add(p); }
            Add(enemy.Moves);
            if (phase >= 1) Add(enemy.PhaseMoves);
            return all;
        }

        /// <summary>n번째 겹이 시작되는 시간 — 소개 턴이면 새 패턴을 먼저 보여 준 뒤</summary>
        public float LayerStartTime(int layer)
        {
            if (layer <= 0 || !encounter || !encounter.profile) return 0f;
            float delay = encounter.profile.LayerDelay(layer);
            return plan.IsIntroTurn ? IntroduceSeconds + delay : delay;
        }

        public void Tick(float deltaTime)
        {
            if (context == null || deltaTime <= 0f) return;
            elapsed += deltaTime;
            if (!state.Introduced && plan.Primary == 0 && elapsed >= IntroduceSeconds) state.Introduced = true;

            for (int layer = 1; layer < plan.LayerCount; layer++)
            {
                if (layerStarted[layer] || elapsed < LayerStartTime(layer)) continue;
                if (LiveThreats() >= encounter.profile.liveThreatCap) break; // 위험이 많으면 다음 겹을 미룬다
                layerStarted[layer] = true;
                Spawn(patterns[plan.Layer(layer)]);
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
                directable.ApplyDifficulty(intervalScale, speedScale);
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
