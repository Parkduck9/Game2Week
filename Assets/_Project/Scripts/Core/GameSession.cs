using System;
using Game2Week.Battle;
using Game2Week.Data;
using UnityEngine;

namespace Game2Week.Core
{
    /// <summary>
    /// 씬 사이에 넘겨야 하는 진행 정보 (어떤 적과 싸우는지, 결과가 무엇인지).
    /// 싱글톤 대신 이 에셋을 각 씬의 컨트롤러가 인스펙터로 참조한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Game Session", fileName = "GameSession")]
    public sealed class GameSession : ScriptableObject
    {
        [SerializeField] PlayerData player;
        [SerializeField] EnemyData defaultEnemy;

        // 런타임 전용 — 에셋에 저장되지 않는다
        [NonSerialized] EnemyData currentEnemy;
        [NonSerialized] BattleOutcome? lastOutcome;

        public PlayerData Player => player;
        public EnemyData CurrentEnemy => currentEnemy ? currentEnemy : defaultEnemy;
        public BattleOutcome? LastOutcome => lastOutcome;

        public void BeginBattle(EnemyData enemy = null)
        {
            currentEnemy = enemy ? enemy : defaultEnemy;
            lastOutcome = null;
        }

        public void EndBattle(BattleOutcome outcome) => lastOutcome = outcome;

        public void ResetSession()
        {
            currentEnemy = null;
            lastOutcome = null;
        }
    }
}
