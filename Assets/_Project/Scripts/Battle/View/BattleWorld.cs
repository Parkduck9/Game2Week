using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Core;
using Game2Week.Data;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>IBattleWorld 구현 — 스폰된 주인공·적·보석·탄막으로 이동·접촉·피격을 처리한다.</summary>
    public sealed class BattleWorld : MonoBehaviour, IBattleWorld
    {
        [SerializeField] BattleEffects effects;
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
        }

        public void ResetPlayer()
        {
            player.Teleport(spawner.Arena.CellToWorld(stage.playerStart));
            invulnerableLeft = 0f;
            pendingDamage = 0;
            player.SetVisible(true);
            actionInput?.ClearPlayerCommands();
            if (actionCamera) actionCamera.ResetFollow();
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
