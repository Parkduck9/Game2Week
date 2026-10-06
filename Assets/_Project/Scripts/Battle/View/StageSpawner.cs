using System.Collections.Generic;
using Game2Week.Data;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>스테이지 데이터대로 경기장을 만들고 주인공·적·보석을 놓는다.</summary>
    public sealed class StageSpawner : MonoBehaviour
    {
        [SerializeField] BattleArena arena;
        [SerializeField] GameObject playerPrefab;
        [Tooltip("EnemyData에 View 프리팹이 없을 때 쓰는 임시 적")]
        [SerializeField] GameObject fallbackEnemyPrefab;
        [SerializeField] GemView gemPrefab;

        readonly Dictionary<string, GemView> gems = new();

        public BattleArena Arena => arena;
        public Transform Player { get; private set; }
        public EnemyView Enemy { get; private set; }
        public IReadOnlyDictionary<string, GemView> Gems => gems;

        public void Spawn(StageDefinition stage, EnemyData enemyData)
        {
            arena.Build(stage.grid);

            Player = Instantiate(playerPrefab, arena.CellToWorld(stage.playerStart), Quaternion.identity, arena.transform).transform;
            Player.name = "Player";

            var enemyPrefab = enemyData && enemyData.ViewPrefab ? enemyData.ViewPrefab : fallbackEnemyPrefab;
            // 적은 주인공(-Z) 쪽을 본다
            var enemyGo = Instantiate(enemyPrefab, arena.CellToWorld(stage.enemy.position), Quaternion.Euler(0f, 180f, 0f), arena.transform);
            enemyGo.name = $"Enemy_{(enemyData ? enemyData.name : "Fallback")}";
            Enemy = enemyGo.GetComponent<EnemyView>();

            gems.Clear();
            foreach (var gem in stage.gems)
            {
                var view = Instantiate(gemPrefab, arena.CellToWorld(gem.position), Quaternion.identity, arena.transform);
                view.Setup(gem);
                gems[gem.id] = view;
            }
        }

        /// <summary>이번 턴에 나올 보석만 보이게 한다.</summary>
        public void ShowGems(IEnumerable<string> activeIds)
        {
            var active = new HashSet<string>(activeIds);
            foreach (var (id, view) in gems)
                if (active.Contains(id)) view.Show();
                else view.Hide();
        }
    }
}
