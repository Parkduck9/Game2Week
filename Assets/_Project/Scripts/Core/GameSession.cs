using System;
using Game2Week.Battle;
using Game2Week.Data;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Core
{
    /// <summary>
    /// 씬 사이에 넘겨야 하는 진행 정보 (몇 번째 스테이지인지, 어떤 적인지, 결과가 무엇인지).
    /// 싱글톤 대신 이 에셋을 각 씬의 컨트롤러가 인스펙터로 참조한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Game Session", fileName = "GameSession")]
    public sealed class GameSession : ScriptableObject
    {
        [SerializeField] PlayerData player;
        [Tooltip("스테이지의 enemyId를 찾지 못했을 때 쓰는 적")]
        [SerializeField] EnemyData defaultEnemy;
        [SerializeField] ContentCatalog catalog;

        // 런타임 전용 — 에셋에 저장되지 않는다
        [NonSerialized] int stageIndex = -1;
        [NonSerialized] StageDefinition currentStage;
        [NonSerialized] EnemyData currentEnemy;
        [NonSerialized] BattleOutcome? lastOutcome;

        /// <summary>테스트용: 스테이지 폴더를 바꿀 때만 사용</summary>
        public string StageDirectoryOverride { get; set; }

        public PlayerData Player => player;
        public ContentCatalog Catalog => catalog;
        public int StageIndex => stageIndex;
        public StageDefinition CurrentStage => currentStage;
        public EnemyData CurrentEnemy => currentEnemy ? currentEnemy : defaultEnemy;
        public BattleOutcome? LastOutcome => lastOutcome;

        StageRepository Repository => new(StageDirectoryOverride ?? StageRepository.DefaultDirectory);

        public int StageCount => Repository.LoadIndex().stages.Count;

        /// <summary>
        /// stages.json 순서의 index번째 스테이지로 전투를 준비한다. 쓸 수 없는 스테이지면 false (오류는 로그).
        /// </summary>
        public bool BeginStage(int index)
        {
            lastOutcome = null;
            currentStage = null;
            currentEnemy = null;
            stageIndex = index;

            var repo = Repository;
            var ids = repo.LoadIndex().stages;
            if (index < 0 || index >= ids.Count)
            {
                Debug.LogError($"스테이지 {index + 1}번이 없음 (전체 {ids.Count}개)");
                return false;
            }

            var result = repo.LoadStage(ids[index], catalog);
            foreach (var issue in result.Issues)
                if (issue.Severity == IssueSeverity.Error) Debug.LogError($"[Stage] {issue}");
                else Debug.LogWarning($"[Stage] {issue}");
            if (!result.IsUsable) return false;

            currentStage = result.Stage;
            currentEnemy = catalog ? catalog.FindEnemy(currentStage.enemy.enemyId) : null;
            return true;
        }

        public void EndBattle(BattleOutcome outcome) => lastOutcome = outcome;

        public void ResetSession()
        {
            stageIndex = -1;
            currentStage = null;
            currentEnemy = null;
            lastOutcome = null;
        }
    }
}
