using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Core;
using Game2Week.Data;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>IBattleWorld 구현 — 스폰된 주인공·적·보석·탄막으로 이동·접촉·피격을 처리한다.</summary>
    public sealed class BattleWorld : MonoBehaviour, IBattleWorld, IActionPresentationWorld
    {
        [SerializeField] BattleEffects effects;
        [SerializeField] CombatPresentationSettings presentationSettings;
        EnemyFlight flight;
        Animation.PlayerAnimationDriver animationDriver;
        bool firstTurn=true;
        Vector3 enemyActionStart,playerActionStart,retreatTarget,runTarget;
        public CombatPresentationSettings PresentationSettings=>presentationSettings;
        public float StrikeImpactTime=>presentationSettings?presentationSettings.strikeImpactTime:.3f;
        public float StrikeHoldSeconds=>presentationSettings?presentationSettings.hitHoldSeconds:.055f;
        [SerializeField] PatternRunner patternRunner;
        [SerializeField] PlayerActionSettings actionSettings;
        [SerializeField] ThreatFeedbackView threatFeedback;
        InputReader actionInput;
        BattleCameraDirector actionCamera;
        [Tooltip("보석을 먹는 거리 (m, 수평)")]
        [SerializeField, Min(0.05f)] float gemPickupRadius = 0.35f;
        [Tooltip("맞은 뒤 무적 시간 (초)")]
        [SerializeField, Min(0f)] float invulnerableSeconds = 1f;
        [SerializeField, Min(1f)] float blinkPerSecond = 12f;

        StageSpawner spawner;
        StageDefinition stage;
        PlayerMover player;
        PlayerHitRule hitRule;
        readonly EncounterMemory encounterMemory = new();
        readonly BattleFeedback feedback = new();
        float invulnerableLeft;
        int pendingDamage;

        public PlayerMover Player => player;
        /// <summary>월드 사건 알림 (이펙트·소리가 구독)</summary>
        public BattleFeedback Feedback => feedback;
        public PlayerHitRule HitRule => hitRule;
        public EncounterMemory EncounterMemory => encounterMemory;
        public PatternRunner Patterns => patternRunner;
        public ThreatFeedbackView ThreatFeedback => threatFeedback;
        public void BindThreatVolume(System.Func<float> volume) { if (threatFeedback) threatFeedback.Bind(patternRunner,player,volume); }
        public bool IsInvulnerable => invulnerableLeft > 0f || (player && player.Motor.DodgeInvulnerable);
        public void ConfigureActions(InputReader input, BattleCameraDirector camera)
        { actionInput = input; actionCamera = camera; camera.BindActions(player.transform, spawner.Enemy.transform, input, spawner.Arena); }

        public void Init(StageSpawner stageSpawner, StageDefinition stageDefinition)
        {
            spawner = stageSpawner;
            stage = stageDefinition;
            if (!spawner.Player.TryGetComponent(out player)) player = spawner.Player.gameObject.AddComponent<PlayerMover>();
            player.Init(spawner.Arena, actionSettings, feedback);
            hitRule = new PlayerHitRule(player, feedback);
            // 리깅된 모델이면 애니메이션 구동기에 피격·쳐내기 방향 알림 연결 (7단계 통합)
            var animation = player.GetComponentInChildren<Animation.PlayerAnimationDriver>();
            if (animation) animation.Bind(player, feedback);
            animationDriver=animation;firstTurn=true;
            if(spawner.Enemy.TryGetComponent(out flight))flight.Configure(spawner.Arena,player.transform,presentationSettings);
            spawner.Enemy.ConfigureMotion(presentationSettings);
        }

        public void ResetPlayer()
        {
            if(firstTurn){player.Teleport(spawner.Arena.CellToWorld(stage.playerStart));if(actionCamera)actionCamera.ResetFollow();firstTurn=false;}
            else player.StopActions();
            invulnerableLeft = 0f;
            pendingDamage = 0;
            player.SetVisible(true);
            actionInput?.ClearPlayerCommands();

        }

        public float PresentationDuration(ActionCue cue)
        {
            if(cue==ActionCue.Strike)return presentationSettings?presentationSettings.strikeDuration:.7f;
            if(cue!=ActionCue.Retreat)return presentationSettings?presentationSettings.actDuration:1.4f;
            var target=RetreatTargetFrom(spawner.Enemy.transform.position,player.transform.position);
            float travel=FlatDistance(target,spawner.Enemy.transform.position);
            if(travel<.05f)return 0;
            return presentationSettings?RetreatPath.Duration(travel,presentationSettings.retreatSpeed,presentationSettings.retreatDuration,presentationSettings.retreatMaxDuration):.9f;
        }
        /// <summary>처음 시작점-적 거리 (m) — 후퇴 거리 기준</summary>
        float StartSeparation=>FlatDistance(spawner.Arena.CellToWorld(stage.playerStart),spawner.Arena.CellToWorld(stage.enemy.position));
        /// <summary>행동 뒤 후퇴 목표: 처음 시작 거리 × 비율만큼 떨어지게 (이미 충분히 멀면 제자리 — 순간이동 없음)</summary>
        Vector3 RetreatTargetFrom(Vector3 enemy,Vector3 playerAt)
        {
            float minimum=presentationSettings?presentationSettings.retreatDistance:4.5f;
            float ratio=presentationSettings?presentationSettings.retreatToStartRatio:.8f;
            float desired=RetreatPath.DesiredSeparation(minimum,StartSeparation,ratio);
            float separation=FlatDistance(enemy,playerAt);
            if(RetreatPath.FarEnough(separation,desired))return enemy;
            return RetreatPath.Target(enemy,playerAt,desired-separation,p=>spawner.Arena.ClampToArena(p,.7f));
        }
        public void BeginPresentation(ActionCue cue)
        {
            if(flight)flight.Pause();player.StopActions();
            enemyActionStart=spawner.Enemy.transform.position;playerActionStart=player.transform.position;
            var direction=enemyActionStart-playerActionStart;direction.y=0;
            if(direction.sqrMagnitude>.001f)player.transform.rotation=Quaternion.LookRotation(direction);
            if(animationDriver)animationDriver.PlayAction(cue);
            spawner.Enemy.SetJointAction(cue);
            retreatTarget=RetreatTargetFrom(enemyActionStart,playerActionStart);
            runTarget=spawner.Arena.ClampToArena(enemyActionStart+player.transform.right*1.2f,.7f);
            if(cue==ActionCue.RunTogether){var runDirection=runTarget-enemyActionStart;if(runDirection.sqrMagnitude>.001f){var facing=Quaternion.LookRotation(runDirection);player.transform.rotation=facing;spawner.Enemy.transform.rotation=facing;}}
        }
        public void UpdatePresentation(ActionCue cue,float progress,bool hold)
        {
            if(animationDriver)animationDriver.HoldAction(hold);
            spawner.Enemy.HoldAction(hold);
            float blend=Mathf.SmoothStep(0,1,progress);
            if(cue==ActionCue.Retreat)spawner.Enemy.transform.position=Vector3.Lerp(enemyActionStart,retreatTarget,blend);
            if(cue==ActionCue.RunTogether)
            {
                var delta=(runTarget-enemyActionStart)*blend;
                spawner.Enemy.transform.position=enemyActionStart+delta;
                player.transform.position=spawner.Arena.ClampToArena(playerActionStart+delta,player.Radius);
            }
            spawner.Enemy.SetJointProgress(progress);
        }
        public void EndPresentation(ActionCue cue)
        {
            if(animationDriver){animationDriver.HoldAction(false);animationDriver.ClearAction();}
            spawner.Enemy.HoldAction(false);spawner.Enemy.ClearJointAction();
        }

        public void MovePlayer(Vector2 input, float deltaTime)
        {
            if (deltaTime <= 0f) { player.Motor.ClearBuffer(); actionInput?.ClearPlayerCommands(); return; }
            if (actionInput)
            {
                player.Motor.SetBrace(actionInput.BraceHeld);
                if (actionInput.ConsumeDodge()) player.Motor.RequestDodge();
                if (actionInput.ConsumeJump()) player.Motor.RequestJump();
                if (actionInput.ConsumeParry()) player.Motor.RequestParry();
            }
            var movement = actionCamera ? actionCamera.ToWorldMove(input) : input;
            Vector3? facing = actionCamera && actionCamera.IsLockedOn ? spawner.Enemy.transform.position - player.transform.position : null;
            player.Move(movement, deltaTime, facing);
        }

        public bool IsPlayerTouchingEnemy()
        {
            var enemy = spawner.Enemy;
            if (!enemy || !player.CanContactEnemy) return false;
            return FlatDistance(player.transform.position, enemy.transform.position) <= enemy.ContactRadius + player.Radius;
        }

        public string TouchedGem(IReadOnlyList<string> visibleGemIds)
        {
            if (!player.Motor.IsGrounded) return null;
            foreach (var id in visibleGemIds)
                if (spawner.Gems.TryGetValue(id, out var gem) &&
                    FlatDistance(player.transform.position, gem.transform.position) <= gemPickupRadius + player.Radius)
                    return id;
            return null;
        }

        public void ShowGems(IReadOnlyList<string> gemIds) => spawner.ShowGems(gemIds);

        public void CollectGem(string gemId)
        {
            if (!spawner.Gems.TryGetValue(gemId, out var gem)) return;
            if (effects) effects.PlayGemPickup(gem.transform.position);
            gem.Hide();
        }

        public void BeginPattern(AttackPatternData pattern, int damagePerHit, EnemyPatternInfo enemyInfo = null)
        {
            pendingDamage = 0;
            if(flight)flight.Begin();
            var enemy = spawner.Enemy;
            var context = new PatternContext(spawner.Arena, enemy ? enemy.transform : spawner.Arena.transform, enemy,
                player.transform, player.Radius, damagePerHit, OnPlayerHit, player, hitRule, encounterMemory, feedback, enemyInfo);
            hitRule.ResetCounts();
            patternRunner.Begin(pattern, context, spawner.Arena.transform);
        }

        public int ConsumePlayerDamage()
        {
            int damage = pendingDamage;
            pendingDamage = 0;
            return damage;
        }

        public void EndPattern()
        {
            patternRunner.End();
            if(flight)flight.Pause();
            if (threatFeedback) threatFeedback.Clear();
            invulnerableLeft = 0f;
            player.SetVisible(true);
            player.StopActions();
            actionInput?.ClearPlayerCommands();
        }

        void OnPlayerHit(int damage)
        {
            if (IsInvulnerable) return;
            pendingDamage += damage;
            invulnerableLeft = invulnerableSeconds;
            feedback.RaisePlayerHit(player.transform.position);
        }

        void Update()
        {
            if (invulnerableLeft <= 0f || !player) return;
            invulnerableLeft -= Time.deltaTime;
            player.SetVisible(invulnerableLeft <= 0f || Mathf.FloorToInt(invulnerableLeft * blinkPerSecond) % 2 == 0);
        }

        static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
