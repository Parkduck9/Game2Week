using System;
using System.Collections.Generic;
using Game2Week.Battle.View;
using Game2Week.Data;
using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 탄막 패턴. 프리팹의 컴포넌트로 구현하고 PatternRunner가 탄막 턴마다 만들어 돌린다.
    /// 시간은 Tick의 deltaTime만 쓴다 (일시정지 때 0).
    /// </summary>
    public interface IAttackPattern
    {
        void Begin(PatternContext context);
        void Tick(float deltaTime);
        /// <summary>남은 탄을 모두 치운다.</summary>
        void End();
    }

    /// <summary>
    /// 이번 탄막 턴의 적 정보 (9단계): 적 고유 기술, 체력 단계에서 더해지는 기술, 현재 체력 비율.
    /// 맵은 "어떤 적 + 난이도"를 정하고, 적은 자기 기술을 더한다.
    /// </summary>
    public sealed class EnemyPatternInfo
    {
        public static readonly EnemyPatternInfo None = new(null, null, null);

        public EnemyPatternInfo(IReadOnlyList<AttackPatternData> moves, IReadOnlyList<AttackPatternData> phaseMoves, Func<float> healthRatio)
        {
            Moves = moves ?? Array.Empty<AttackPatternData>();
            PhaseMoves = phaseMoves ?? Array.Empty<AttackPatternData>();
            this.healthRatio = healthRatio;
        }

        readonly Func<float> healthRatio;
        /// <summary>항상 쓰는 적 고유 기술</summary>
        public IReadOnlyList<AttackPatternData> Moves { get; }
        /// <summary>체력 단계 1 이상에서 더해지는 기술</summary>
        public IReadOnlyList<AttackPatternData> PhaseMoves { get; }
        /// <summary>적 체력 비율 (0~1). 모르면 1.</summary>
        public float HealthRatio => healthRatio != null ? Mathf.Clamp01(healthRatio()) : 1f;
    }

    /// <summary>패턴이 쓸 수 있는 정보: 경기장 경계, 적 위치, 주인공 위치, 맞았을 때 알리기.</summary>
    public sealed class PatternContext
    {
        readonly Action<int> reportHit;
        readonly IHitRule hitRule;

        public PatternContext(BattleArena arena, Transform enemy, EnemyView enemyView, Transform player, float playerRadius, int damagePerHit, Action<int> reportHit, PlayerMover playerMover = null, IHitRule hitRule = null, EncounterMemory memory = null, BattleFeedback feedback = null, EnemyPatternInfo enemyInfo = null)
        {
            this.hitRule = hitRule;
            Memory = memory ?? new EncounterMemory();
            Feedback = feedback ?? new BattleFeedback();
            EnemyInfo = enemyInfo ?? EnemyPatternInfo.None;
            Arena = arena;
            Enemy = enemy;
            EnemyView = enemyView;
            Player = player;
            PlayerRadius = playerRadius;
            DamagePerHit = damagePerHit;
            this.reportHit = reportHit;
            PlayerMover = playerMover;
        }

        public BattleArena Arena { get; }
        public Transform Enemy { get; }
        /// <summary>발사 순간 공격 모션용 (없을 수 있음)</summary>
        public EnemyView EnemyView { get; }
        public Transform Player { get; }
        public float PlayerRadius { get; }
        public int DamagePerHit { get; }
        public PlayerMover PlayerMover { get; }
        /// <summary>전투 동안 유지되는 패턴 기록 (턴 간 소개 여부·셔플 백). 없으면 이 턴 전용으로 새로 만든다.</summary>
        public EncounterMemory Memory { get; }
        /// <summary>연출 알림 — 패턴은 발사(RaiseBulletFired)·예고(RaiseWarningStarted)를 알린다. 없으면 아무도 안 듣는 빈 창구.</summary>
        public BattleFeedback Feedback { get; }
        /// <summary>적 고유 기술·체력 비율 (9단계). 없으면 빈 정보 (체력 1).</summary>
        public EnemyPatternInfo EnemyInfo { get; }
        public Vector3 PreviousPlayerPosition => PlayerMover ? PlayerMover.PreviousPosition : Player.position;
        /// <summary>주인공 수평 속도 (예측 조준용, 없으면 0)</summary>
        public Vector3 PlayerVelocity => PlayerMover ? PlayerMover.GroundVelocity : Vector3.zero;
        public float PlayerBodyHeight => PlayerMover ? PlayerMover.BodyHeight : 0.85f;

        /// <summary>탄이 주인공에게 닿았을 때 호출 (무적 시간 처리는 받는 쪽에서)</summary>
        public void ReportHit() => reportHit?.Invoke(DamagePerHit);

        /// <summary>몸과 겹친 color 탄이 실제로 피해를 주는지 (색 규칙, 없으면 항상 true)</summary>
        public bool ShouldHit(AttackColor color) => hitRule == null || hitRule.ShouldHit(color);

        /// <summary>경기장 바깥으로 margin 이상 나갔는지 (탄 회수용)</summary>
        public bool IsOutside(Vector3 world, float margin)
        {
            var local = Arena.transform.InverseTransformPoint(world);
            return Mathf.Abs(local.x) > Arena.Size.x * 0.5f + margin || Mathf.Abs(local.z) > Arena.Size.y * 0.5f + margin;
        }

        public static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
