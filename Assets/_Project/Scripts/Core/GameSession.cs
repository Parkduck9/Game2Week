using System;
using System.Collections.Generic;
using Game2Week.Battle;
using Game2Week.Data;
using Game2Week.Save;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Core
{
    /// <summary>
    /// 씬 사이에 넘겨야 하는 진행 정보 (몇 번째 스테이지인지, 어떤 적인지, 결과·기록이 무엇인지) + 세이브.
    /// 싱글톤 대신 이 에셋을 각 씬의 컨트롤러가 인스펙터로 참조한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Game Session", fileName = "GameSession")]
    public sealed class GameSession : ScriptableObject
    {
        /// <summary>테스트 전용: 세이브 폴더를 임시 폴더로 (null이면 persistentDataPath)</summary>
        public static string SaveDirectoryOverrideForTests { get; set; }

        [SerializeField] PlayerData player;
        [Tooltip("스테이지의 enemyId를 찾지 못했을 때 쓰는 적")]
        [SerializeField] EnemyData defaultEnemy;
        [SerializeField] ContentCatalog catalog;

        // 런타임 전용 — 에셋에 저장되지 않는다
        [NonSerialized] int stageIndex = -1;
        [NonSerialized] StageDefinition currentStage;
        [NonSerialized] EnemyData currentEnemy;
        [NonSerialized] BattleOutcome? lastOutcome;
        [NonSerialized] SaveService save;
        [NonSerialized] string saveDirectory;
        [NonSerialized] bool settingsApplied;

        /// <summary>테스트용: 스테이지 폴더를 바꿀 때만 사용</summary>
        public string StageDirectoryOverride { get; set; }

        public PlayerData Player => player;
        public ContentCatalog Catalog => catalog;
        public int StageIndex => stageIndex;
        public StageDefinition CurrentStage => currentStage;
        public EnemyData CurrentEnemy => currentEnemy ? currentEnemy : defaultEnemy;
        public BattleOutcome? LastOutcome => lastOutcome;
        public float LastClearTime { get; private set; }
        public bool LastIsNewRecord { get; private set; }

        /// <summary>진행·설정 저장소. 세이브 폴더가 바뀌면(테스트) 다시 읽는다.</summary>
        public SaveService Save
        {
            get
            {
                var dir = SaveDirectoryOverrideForTests ?? SaveService.DefaultDirectory;
                if (save == null || saveDirectory != dir)
                {
                    save = new SaveService(dir);
                    saveDirectory = dir;
                }
                return save;
            }
        }

        StageRepository Repository => new(StageDirectoryOverride ?? StageRepository.DefaultDirectory);

        /// <summary>진행 순서대로 스테이지 id (stages.json)</summary>
        public IReadOnlyList<string> StageIds => Repository.LoadIndex().stages;

        public int StageCount => StageIds.Count;

        public bool IsLastStage => stageIndex >= 0 && stageIndex == StageCount - 1;

        public bool HasNextStage => stageIndex >= 0 && stageIndex + 1 < Save.UnlockedCount(StageIds);

        /// <summary>게임 시작 시 한 번: 저장된 화면 설정 적용.</summary>
        public void ApplySettingsOnce()
        {
            if (settingsApplied) return;
            settingsApplied = true;
            DisplaySettings.Apply(Save.Settings);
        }

        /// <summary>스테이지 이름 (선택 화면용). 못 읽으면 id.</summary>
        public string StageName(string stageId)
        {
            var result = Repository.LoadStage(stageId, catalog);
            return result.Stage != null ? result.Stage.name : stageId;
        }

        /// <summary>
        /// stages.json 순서의 index번째 스테이지로 전투를 준비한다. 쓸 수 없는 스테이지면 false (오류는 로그).
        /// </summary>
        public bool BeginStage(int index)
        {
            lastOutcome = null;
            currentStage = null;
            currentEnemy = null;
            LastClearTime = 0f;
            LastIsNewRecord = false;
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

        /// <summary>전투 종료: 이기면 클리어 기록을 저장한다.</summary>
        public void EndBattle(BattleOutcome outcome, float clearTimeSeconds = 0f)
        {
            lastOutcome = outcome;
            // 0초는 "기록 없음"과 구분이 안 되므로 최소 0.1초
            LastClearTime = Mathf.Max(0.1f, clearTimeSeconds);
            LastIsNewRecord = false;
            if (outcome != BattleOutcome.PlayerDefeated && currentStage != null)
                LastIsNewRecord = Save.RecordClear(currentStage.id, LastClearTime, outcome.ToString());
        }

        public void ResetSession()
        {
            stageIndex = -1;
            currentStage = null;
            currentEnemy = null;
            lastOutcome = null;
            LastClearTime = 0f;
            LastIsNewRecord = false;
        }
    }
}
