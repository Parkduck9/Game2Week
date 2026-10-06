using System;
using Game2Week.Battle.View;
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

    /// <summary>패턴이 쓸 수 있는 정보: 경기장 경계, 적 위치, 주인공 위치, 맞았을 때 알리기.</summary>
    public sealed class PatternContext
    {
        readonly Action<int> reportHit;
        readonly IHitRule hitRule;

        public PatternContext(BattleArena arena, Transform enemy, EnemyView enemyView, Transform player, float playerRadius, int damagePerHit, Action<int> reportHit, PlayerMover playerMover = null, IHitRule hitRule = null)
        {
            this.hitRule = hitRule;
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
        public Vector3 PreviousPlayerPosition => PlayerMover ? PlayerMover.PreviousPosition : Player.position;
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
