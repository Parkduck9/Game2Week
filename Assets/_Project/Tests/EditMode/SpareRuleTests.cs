using System.Collections.Generic;
using Game2Week.Battle;
using NUnit.Framework;

namespace Game2Week.Tests
{
    public class SpareRuleTests
    {
        static SpareTracker Tracker(params ISpareCondition[] conditions) => new(new SpareRule { enabled = true, conditions = new List<ISpareCondition>(conditions) });
        [Test] public void 세턴과순서를모두채워야자비가능()
        {
            var tracker = Tracker(new ActSequenceCondition { acts = new List<string> { "응원하기", "말 걸기" } });
            tracker.RecordAct("말 걸기"); tracker.RecordAct("응원하기"); tracker.CompletePlayerTurn(); tracker.RecordAct("말 걸기"); tracker.CompletePlayerTurn();
            Assert.IsFalse(tracker.Ready); tracker.CompletePlayerTurn(); Assert.IsTrue(tracker.Ready);
        }
        [TestCase(false, 0)] [TestCase(true, 2)]
        public void 공격선택은누적턴을초기화하거나절반으로줄인다(bool half, int expected)
        {
            var tracker = new SpareTracker(new SpareRule { enabled = true, halveOnFight = half });
            for (int i = 0; i < 4; i++) tracker.CompletePlayerTurn();
            tracker.CompletePlayerTurn(true); Assert.AreEqual(expected, tracker.NoFightTurns);
        }
        [Test] public void 보석은진행도를대신하지않고아직안채운힌트를하나씩공개()
        {
            var tracker = Tracker(new BulletActionCondition { required = 3 });
            StringAssert.Contains("공격하지 않은", tracker.RevealGemHint());
            StringAssert.Contains("쳐내기", tracker.RevealGemHint());
            Assert.AreEqual(0, tracker.NoFightTurns); Assert.AreEqual(2, tracker.Gems); Assert.IsFalse(tracker.Ready);
        }
        [Test] public void 탄막행동은탄막턴에서만세고안맞기와색통과를독립추적()
        {
            var tracker = Tracker(new UnhurtTurnCondition(), new BulletActionCondition { action = SpareActionKind.RedPass, required = 1 });
            tracker.RecordAction(SpareActionKind.RedPass); Assert.AreEqual(0, tracker.ActionCount(SpareActionKind.RedPass));
            tracker.BeginBulletTurn(); tracker.RecordAction(SpareActionKind.RedPass); tracker.RecordHit(); tracker.EndBulletTurn(); Assert.AreEqual(0, tracker.UnhurtTurns);
            tracker.BeginBulletTurn(); tracker.EndBulletTurn(); Assert.AreEqual(1, tracker.UnhurtTurns);
            for (int i = 0; i < 3; i++) tracker.CompletePlayerTurn(); Assert.IsTrue(tracker.Ready);
        }
        [Test] public void 선택플래그보석체력조건은모두만족해야한다()
        {
            var tracker = Tracker(new DialogueFlagCondition { flag = "heard" }, new SpareGemCondition { required = 1 }, new EnemyHealthCondition { ratio = .5f });
            for (int i = 0; i < 3; i++) tracker.CompletePlayerTurn(); tracker.SetFlag("heard"); tracker.RevealGemHint();
            Assert.IsFalse(tracker.Ready); tracker.HealthRatio = .5f; Assert.IsTrue(tracker.Ready); tracker.SetFlag("heard", false); Assert.IsFalse(tracker.Ready);
        }
        [Test] public void 같은에셋조건을두전투가공유해도진행도는독립()
        {
            var rule = new SpareRule { enabled = true }; var first = new SpareTracker(rule); var second = new SpareTracker(rule);
            first.CompletePlayerTurn(); Assert.AreEqual(0, second.NoFightTurns);
        }
    }
}
