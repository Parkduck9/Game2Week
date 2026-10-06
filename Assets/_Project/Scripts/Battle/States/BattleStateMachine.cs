using System;
using System.Collections.Generic;

namespace Game2Week.Battle
{
    /// <summary>
    /// 전투 흐름을 관리한다. 상태 전환은 반드시 여기를 거친다.
    /// Enter/Exit 도중에 들어온 전환 요청은 현재 전환이 끝난 뒤 순서대로 처리한다.
    /// </summary>
    public sealed class BattleStateMachine
    {
        readonly Dictionary<BattleStateId, IBattleState> states = new();
        readonly Queue<BattleStateId> pending = new();
        IBattleState current;
        bool isTransitioning;

        /// <summary>(이전 상태, 새 상태). 첫 전환에서 이전 상태는 null.</summary>
        public event Action<BattleStateId?, BattleStateId> StateChanged;

        public BattleStateId? CurrentId { get; private set; }

        public void Register(BattleStateId id, IBattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!states.TryAdd(id, state)) throw new InvalidOperationException($"이미 등록된 전투 상태: {id}");
        }

        public bool IsRegistered(BattleStateId id) => states.ContainsKey(id);

        public void ChangeState(BattleStateId id)
        {
            if (!states.ContainsKey(id)) throw new InvalidOperationException($"등록되지 않은 전투 상태: {id}");

            pending.Enqueue(id);
            if (isTransitioning) return;

            isTransitioning = true;
            try
            {
                while (pending.Count > 0) Transition(pending.Dequeue());
            }
            finally
            {
                isTransitioning = false;
                pending.Clear();
            }
        }

        public void Tick(float deltaTime) => current?.Tick(deltaTime);

        void Transition(BattleStateId id)
        {
            var previous = CurrentId;
            current?.Exit();
            current = states[id];
            CurrentId = id;
            StateChanged?.Invoke(previous, id);
            current.Enter();
        }
    }
}
