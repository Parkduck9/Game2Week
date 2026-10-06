using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 전투 이벤트 → 3D 연출 (카메라 샷, 적 애니메이션, 이펙트). 로직은 이 클래스를 모른다.
    /// </summary>
    public sealed class BattlePresentation : MonoBehaviour
    {
        [SerializeField] BattleCameraDirector cameraDirector;
        [SerializeField] BattleEffects effects;

        BattleEvents events;
        EnemyView enemy;

        public static BattleShot ShotFor(BattleStateId state) => state switch
        {
            BattleStateId.Intro => BattleShot.Intro,
            BattleStateId.EnemyTurn => BattleShot.Overview,
            BattleStateId.Fight => BattleShot.AttackCloseUp,
            _ => BattleShot.EnemyFocus,
        };

        public void Bind(BattleEvents battleEvents, EnemyView enemyView)
        {
            Unbind();
            events = battleEvents;
            enemy = enemyView;
            events.StateChanged += OnStateChanged;
            events.EnemyDamaged += OnEnemyDamaged;
            events.PlayerDamaged += OnPlayerDamaged;
            events.BattleEnded += OnBattleEnded;
        }

        void OnDestroy() => Unbind();

        void Unbind()
        {
            if (events == null) return;
            events.StateChanged -= OnStateChanged;
            events.EnemyDamaged -= OnEnemyDamaged;
            events.PlayerDamaged -= OnPlayerDamaged;
            events.BattleEnded -= OnBattleEnded;
            events = null;
        }

        void OnStateChanged(BattleStateId state) => cameraDirector.Show(ShotFor(state));

        void OnEnemyDamaged(int amount)
        {
            if (amount <= 0 || !enemy) return;
            enemy.PlayHit();
            effects.PlayHit(enemy.transform.position + Vector3.up * 0.6f);
        }

        void OnPlayerDamaged(int amount) => cameraDirector.Shake(0.6f);

        void OnBattleEnded(BattleOutcome outcome)
        {
            if (!enemy) return;
            if (outcome == BattleOutcome.EnemyDefeated) enemy.PlayDefeated();
            else if (outcome == BattleOutcome.EnemySpared) enemy.PlaySpared();
        }
    }
}
