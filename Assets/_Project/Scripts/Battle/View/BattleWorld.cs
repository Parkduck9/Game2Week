using System.Collections.Generic;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>IBattleWorld 구현 — 스폰된 주인공·적·보석으로 이동과 접촉을 처리한다.</summary>
    public sealed class BattleWorld : MonoBehaviour, IBattleWorld
    {
        [SerializeField] BattleEffects effects;
        [Tooltip("보석을 먹는 거리 (m, 수평)")]
        [SerializeField, Min(0.05f)] float gemPickupRadius = 0.35f;

        StageSpawner spawner;
        StageDefinition stage;
        PlayerMover player;

        public PlayerMover Player => player;

        public void Init(StageSpawner stageSpawner, StageDefinition stageDefinition)
        {
            spawner = stageSpawner;
            stage = stageDefinition;
            if (!spawner.Player.TryGetComponent(out player)) player = spawner.Player.gameObject.AddComponent<PlayerMover>();
            player.Init(spawner.Arena);
        }

        public void ResetPlayer() => player.Teleport(spawner.Arena.CellToWorld(stage.playerStart));

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

        static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
