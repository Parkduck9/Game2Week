using Game2Week.Core;

namespace Game2Week.Battle
{
    /// <summary>
    /// 상태들이 공유하는 전투 정보. 상태는 이 객체만 알고, 서로를 직접 참조하지 않는다.
    /// 전투원(플레이어·적), UI, 전투 박스 참조는 해당 단계에서 추가한다.
    /// </summary>
    public sealed class BattleContext
    {
        readonly BattleStateMachine stateMachine;

        public BattleContext(InputReader input, BattleEvents events, BattleStateMachine stateMachine)
        {
            Input = input;
            Events = events;
            this.stateMachine = stateMachine;
        }

        public InputReader Input { get; }

        public BattleEvents Events { get; }

        public BattleStateId? CurrentState => stateMachine.CurrentId;

        public void ChangeState(BattleStateId id) => stateMachine.ChangeState(id);
    }
}
