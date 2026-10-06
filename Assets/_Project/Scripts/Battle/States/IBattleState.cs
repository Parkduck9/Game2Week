namespace Game2Week.Battle
{
    public interface IBattleState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

    /// <summary>
    /// 상태 구현의 기본 클래스. 필요한 것은 모두 <see cref="BattleContext"/>를 통해 얻는다.
    /// </summary>
    public abstract class BattleStateBase : IBattleState
    {
        protected BattleStateBase(BattleContext context)
        {
            Context = context;
        }

        protected BattleContext Context { get; }

        public virtual void Enter() { }

        public virtual void Tick(float deltaTime) { }

        public virtual void Exit() { }
    }
}
