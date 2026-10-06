using System.Collections.Generic;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Data
{
    /// <summary>
    /// 스테이지 JSON의 이름(enemyId, 패턴 이름) → 실제 에셋. 맵툴의 "카탈로그 새로고침"이 목록을 채운다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Content Catalog", fileName = "ContentCatalog")]
    public sealed class ContentCatalog : ScriptableObject, IStageCatalog
    {
        [SerializeField] List<EnemyData> enemies = new();
        [SerializeField] List<AttackPatternData> patterns = new();

        public IReadOnlyList<EnemyData> Enemies => enemies;
        public IReadOnlyList<AttackPatternData> Patterns => patterns;

        public EnemyData FindEnemy(string enemyId) => enemies.Find(e => e && e.name == enemyId);

        public AttackPatternData FindPattern(string patternId) => patterns.Find(p => p && p.name == patternId);

        public bool HasEnemy(string enemyId) => FindEnemy(enemyId) != null;

        public bool HasPattern(string patternId) => FindPattern(patternId) != null;

        public void SetContent(IEnumerable<EnemyData> enemyAssets, IEnumerable<AttackPatternData> patternAssets)
        {
            enemies = new List<EnemyData>(enemyAssets);
            patterns = new List<AttackPatternData>(patternAssets);
        }
    }
}
