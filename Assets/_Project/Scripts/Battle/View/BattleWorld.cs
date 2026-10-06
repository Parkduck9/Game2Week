using System.Collections.Generic;
using Game2Week.Battle.Patterns;
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
        [Tooltip("보석을 먹는 거리 (m, 수평)")]
        [SerializeField, Min(0.05f)] float gemPickupRadius = 0.35f;
        [Tooltip("맞은 뒤 무적 시간 (초)")]
        [SerializeField, Min(0f)] float invulnerableSeconds = 1f;
        [SerializeField, Min(1f)] float blinkPerSecond = 12f;

        StageSpawner spawner;
        StageDefinition stage;
        PlayerMover player;
        float invulnerableLeft;
        int pendingDamage;

        public PlayerMover Player => player;
        public PatternRunner Patterns => patternRunner;
        public bool IsInvulnerable => invulnerableLeft > 0f;

        public void Init(StageSpawner stageSpawner, StageDefinition stageDefinition)
        {
            spawner = stageSpawner;
            stage = stageDefinition;
            if (!spawner.Player.TryGetComponent(out player)) player = spawner.Player.gameObject.AddComponent<PlayerMover>();
            player.Init(spawner.Arena);
        }

        public void ResetPlayer()
        {
            player.Teleport(spawner.Arena.CellToWorld(stage.playerStart));
            invulnerableLeft = 0f;
            pendingDamage = 0;
            player.SetVisible(true);
        }

        public void MovePlayer(Vector2 input, float deltaTime) => player.Move(input, deltaTime);

        public bool IsPlayerTouchingEnemy()
        {
            var enemy = spawner.Enemy;
            if (!enemy) return false;
            return FlatDistance(player.transform.position, enemy.transform.position) <= enemy.ContactRadius + player.Radius;
        }

        public string TouchedGem(IReadOnlyList<string> visibleGemIds)
        {
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

        public void BeginPattern(AttackPatternData pattern, int damagePerHit)
        {
            pendingDamage = 0;
            var enemy = spawner.Enemy;
            var context = new PatternContext(spawner.Arena, enemy ? enemy.transform : spawner.Arena.transform, enemy,
                player.transform, player.Radius, damagePerHit, OnPlayerHit);
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
            invulnerableLeft = 0f;
            player.SetVisible(true);
        }

        void OnPlayerHit(int damage)
        {
            if (invulnerableLeft > 0f) return;
            pendingDamage += damage;
            invulnerableLeft = invulnerableSeconds;
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
