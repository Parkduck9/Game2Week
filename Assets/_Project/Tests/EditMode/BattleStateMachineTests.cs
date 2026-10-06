using System;
using System.Collections.Generic;
using Game2Week.Battle;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class BattleStateMachineTests
    {
        sealed class RecordingState : IBattleState
        {
            readonly string name;
            readonly List<string> log;

            public RecordingState(string name, List<string> log)
            {
                this.name = name;
                this.log = log;
            }

            public Action OnEnter { get; set; }

            public void Enter()
            {
                log.Add($"{name}.Enter");
                OnEnter?.Invoke();
            }

            public void Tick(float deltaTime) => log.Add($"{name}.Tick");

            public void Exit() => log.Add($"{name}.Exit");
        }

        List<string> log;
        BattleStateMachine machine;

        [SetUp]
        public void SetUp()
        {
            log = new List<string>();
            machine = new BattleStateMachine();
        }

        [Test]
        public void ChangeState_ExitsPreviousThenEntersNext()
        {
            machine.Register(BattleStateId.Intro, new RecordingState("Intro", log));
            machine.Register(BattleStateId.PlayerMenu, new RecordingState("Menu", log));

            machine.ChangeState(BattleStateId.Intro);
            machine.ChangeState(BattleStateId.PlayerMenu);

            CollectionAssert.AreEqual(new[] { "Intro.Enter", "Intro.Exit", "Menu.Enter" }, log);
            Assert.AreEqual(BattleStateId.PlayerMenu, machine.CurrentId);
        }

        [Test]
        public void ChangeStateInsideEnter_RunsAfterCurrentTransitionCompletes()
        {
            var intro = new RecordingState("Intro", log);
            machine.Register(BattleStateId.Intro, intro);
            machine.Register(BattleStateId.PlayerMenu, new RecordingState("Menu", log));
            intro.OnEnter = () => machine.ChangeState(BattleStateId.PlayerMenu);

            machine.ChangeState(BattleStateId.Intro);

            CollectionAssert.AreEqual(new[] { "Intro.Enter", "Intro.Exit", "Menu.Enter" }, log);
            Assert.AreEqual(BattleStateId.PlayerMenu, machine.CurrentId);
        }

        [Test]
        public void StateChanged_ReportsPreviousAndNext()
        {
            machine.Register(BattleStateId.Intro, new RecordingState("Intro", log));
            machine.Register(BattleStateId.EnemyTurn, new RecordingState("Enemy", log));
            var changes = new List<(BattleStateId?, BattleStateId)>();
            machine.StateChanged += (from, to) => changes.Add((from, to));

            machine.ChangeState(BattleStateId.Intro);
            machine.ChangeState(BattleStateId.EnemyTurn);

            CollectionAssert.AreEqual(new (BattleStateId?, BattleStateId)[]
            {
                (null, BattleStateId.Intro),
                (BattleStateId.Intro, BattleStateId.EnemyTurn),
            }, changes);
        }

        [Test]
        public void Tick_ForwardsToCurrentStateOnly()
        {
            machine.Register(BattleStateId.Intro, new RecordingState("Intro", log));
            machine.Tick(0.1f);
            Assert.IsEmpty(log);

            machine.ChangeState(BattleStateId.Intro);
            machine.Tick(0.1f);

            CollectionAssert.AreEqual(new[] { "Intro.Enter", "Intro.Tick" }, log);
        }

        [Test]
        public void ChangeState_ToUnregisteredState_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => machine.ChangeState(BattleStateId.Victory));
        }

        [Test]
        public void Register_SameIdTwice_Throws()
        {
            machine.Register(BattleStateId.Intro, new RecordingState("A", log));
            Assert.Throws<InvalidOperationException>(() => machine.Register(BattleStateId.Intro, new RecordingState("B", log)));
        }
    }
}
