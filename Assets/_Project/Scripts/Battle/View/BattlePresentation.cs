using UnityEngine;
using System;
using System.Collections.Generic;
using Game2Week.Battle.View.Animation;

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
        PlayerAnimationDriver animationDriver;
        int playerMaxHp;
        Game2Week.Data.CombatPresentationSettings combatSettings;
        readonly Dictionary<string,Func<string,bool>> poseActors=new(StringComparer.OrdinalIgnoreCase);

        public static BattleShot ShotFor(BattleStateId state) => state switch
        {
            BattleStateId.Intro => BattleShot.Intro,
            BattleStateId.EnemyTurn => BattleShot.Overview,
            BattleStateId.Fight => BattleShot.AttackCloseUp,
            BattleStateId.ActionPresentation => BattleShot.AttackCloseUp,
            BattleStateId.Defeat => BattleShot.Overview,
            _ => BattleShot.EnemyFocus,
        };

        public void Bind(BattleEvents battleEvents, EnemyView enemyView, PlayerMover playerMover,int maxHp=20,Game2Week.Data.CombatPresentationSettings settings=null)
        {
            Unbind();
            events = battleEvents;
            enemy = enemyView;
            player = playerMover;playerMaxHp=maxHp;combatSettings=settings;
            animationDriver=player?player.GetComponentInChildren<PlayerAnimationDriver>():null;
            poseActors.Clear();
            if(animationDriver){poseActors["heroine"]=animationDriver.PlayDialoguePose;poseActors["player"]=animationDriver.PlayDialoguePose;}
            if(enemy)poseActors["enemy"]=enemy.PlayDialoguePose;
            events.StateChanged += OnStateChanged;
            events.EnemyDamaged += OnEnemyDamaged;
            events.PlayerDamaged += OnPlayerDamaged;
            events.BattleEnded += OnBattleEnded;
            events.DialogueCue += OnDialogueCue;
        }

        void OnDestroy() => Unbind();

        void Unbind()
        {
            if (events == null) return;
            events.StateChanged -= OnStateChanged;
            events.EnemyDamaged -= OnEnemyDamaged;
            events.PlayerDamaged -= OnPlayerDamaged;
            events.BattleEnded -= OnBattleEnded;
            events.DialogueCue -= OnDialogueCue;
            events = null;
        }

        void ClearPoses(){if(animationDriver)animationDriver.ClearDialoguePose();if(enemy)enemy.ClearDialoguePose();}
        void OnStateChanged(BattleStateId state){ClearPoses();cameraDirector.Show(ShotFor(state));}
        void OnDialogueCue(string anchor, string pose, string camera)
        {
            if (!string.IsNullOrEmpty(camera)) cameraDirector.ShowDialogueCut(camera);
            ClearPoses();
            if(!string.IsNullOrEmpty(pose)&&(!poseActors.TryGetValue(anchor??string.Empty,out var apply)||!apply(pose)))
                Debug.LogWarning("등록되지 않은 대화 자세 또는 화자: "+anchor+" / "+pose);
        }

        void OnEnemyDamaged(int amount)
        {
            if (amount <= 0 || !enemy) return;
            enemy.PlayHit();enemy.Recoil(enemy.transform.position-player.transform.position);
            cameraDirector.Shake(combatSettings?combatSettings.shakeForce:2f);
            var pos = enemy.transform.position + Vector3.up * 0.6f;
            var cam = Camera.main;
            if (cam) pos += (cam.transform.position - pos).normalized * hitTowardCamera;
            effects.PlayHit(pos);
        }

        void OnPlayerDamaged(int amount){if(amount<=0)return;cameraDirector.Shake(0.6f);if(animationDriver)animationDriver.PlayDamage(amount,playerMaxHp);}

        void OnBattleEnded(BattleOutcome outcome)
        {
            if(animationDriver)animationDriver.PlayOutcome(outcome);
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
