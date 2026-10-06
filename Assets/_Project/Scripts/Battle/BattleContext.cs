using System;
using Game2Week.Core;
using Game2Week.Data;
using Game2Week.Stages;

namespace Game2Week.Battle
{
    /// <summary>
    /// 상태들이 공유하는 전투 정보. 상태는 이 객체만 알고, 서로를 직접 참조하지 않는다.
    /// 화면·3D는 IBattleUi / IBattleWorld 인터페이스로만 다룬다 (테스트에서 가짜로 바꿀 수 있게).
    /// </summary>
    public sealed class BattleContext
    {
        readonly BattleStateMachine stateMachine;
        readonly Action<BattleOutcome> finish;

        public BattleContext(
            BattleStateMachine stateMachine,
            BattleEvents events,
            InputReader input,
            IBattleUi ui,
            IBattleWorld world,
            StageDefinition stage,
            PlayerCombatant player,
            EnemyCombatant enemy,
            GemField gems,
            GemRewardSettings gemRewards,
            Action<BattleOutcome> finish,
            Func<AttackPatternData> nextPattern = null)
        {
            NextPattern = nextPattern ?? enemy.NextPattern;
            this.stateMachine = stateMachine;
            Events = events;
            Input = input;
            Ui = ui;
            World = world;
            Stage = stage;
            Player = player;
            Enemy = enemy;
            Gems = gems;
            RewardSettings = gemRewards;
            this.finish = finish;
        }

        public BattleEvents Events { get; }
        /// <summary>테스트에서는 null일 수 있다 (입력 대신 가짜 World/UI가 응답).</summary>
        public InputReader Input { get; }
        public IBattleUi Ui { get; }
        public IBattleWorld World { get; }
        public StageDefinition Stage { get; }
        public PlayerCombatant Player { get; }
        public EnemyCombatant Enemy { get; }
        public GemField Gems { get; }
        public GemRewardSettings RewardSettings { get; }

        /// <summary>다음 탄막 턴에 쓸 패턴 (스테이지 지정 패턴 → 없으면 적 데이터 순서)</summary>
        public Func<AttackPatternData> NextPattern { get; }

        public BattleStateId? CurrentState => stateMachine.CurrentId;

        public void ChangeState(BattleStateId id) => stateMachine.ChangeState(id);

        /// <summary>전투를 끝내고 결과 화면으로 (BattleController가 처리).</summary>
        public void FinishBattle(BattleOutcome outcome) => finish?.Invoke(outcome);

        public void RaisePlayerHp() => Events.RaisePlayerHpChanged(Player.CurrentHp, Player.MaxHp);

        public void RaiseEnemyHp() => Events.RaiseEnemyHpChanged(Enemy.CurrentHp, Enemy.MaxHp);
    }
}
