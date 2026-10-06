using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 전투 이벤트 → 3D 연출 (카메라 샷, 적·주인공 연출, 이펙트). 로직은 이 클래스를 모른다.
    /// </summary>
    public sealed class BattlePresentation : MonoBehaviour
    {
        [SerializeField] BattleCameraDirector cameraDirector;
        [SerializeField] BattleEffects effects;
        [Tooltip("피격 파티클을 카메라 쪽으로 당기는 거리 (m) — 적 몸에 묻히지 않게")]
        [SerializeField] float hitTowardCamera = 0.45f;

        BattleEvents events;
        EnemyView enemy;
        PlayerMover player;

        public static BattleShot ShotFor(BattleStateId state) => state switch
        {
            BattleStateId.Intro => BattleShot.Intro,
            BattleStateId.EnemyTurn => BattleShot.Overview,
            BattleStateId.Fight => BattleShot.AttackCloseUp,
            BattleStateId.Defeat => BattleShot.Overview,
            _ => BattleShot.EnemyFocus,
        };

        public void Bind(BattleEvents battleEvents, EnemyView enemyView, PlayerMover playerMover)
        {
            Unbind();
            events = battleEvents;
            enemy = enemyView;
            player = playerMover;
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
            var pos = enemy.transform.position + Vector3.up * 0.6f;
            var cam = Camera.main;
            if (cam) pos += (cam.transform.position - pos).normalized * hitTowardCamera;
            effects.PlayHit(pos);
        }

        void OnPlayerDamaged(int amount) => cameraDirector.Shake(0.6f);

        void OnBattleEnded(BattleOutcome outcome)
        {
            switch (outcome)
            {
                case BattleOutcome.EnemyDefeated:
                    if (enemy) enemy.PlayDefeated();
                    break;
                case BattleOutcome.EnemySpared:
                    if (enemy) enemy.PlaySpared();
                    break;
                case BattleOutcome.PlayerDefeated:
                    // 패배 연출: (리깅된 모델이면) 쓰러지는 동작 → 파편으로 흩어지며 사라짐
                    if (!player) break;
                    var driver = player.GetComponentInChildren<Animation.PlayerAnimationDriver>();
                    if (driver && driver.IsReady)
                    {
                        driver.PlayFall();
                        StartCoroutine(VanishAfter(FallSeconds));
                    }
                    else Vanish();
                    break;
            }
        }

        /// <summary>쓰러지는 동작을 보여 주는 시간 — 결과 화면 전환(패배 상태) 전에 끝나야 함</summary>
        const float FallSeconds = 0.45f;

        System.Collections.IEnumerator VanishAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Vanish();
        }

        void Vanish()
        {
            if (!player) return;
            effects.PlayHit(player.transform.position + Vector3.up * 0.5f, 1.5f);
            player.SetVisible(false);
        }
    }
}
