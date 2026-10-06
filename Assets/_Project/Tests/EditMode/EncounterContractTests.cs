using Game2Week.Battle.Patterns;
using NUnit.Framework;

namespace Game2Week.Tests
{
    /// <summary>5단계 연결 지점: 색 조합 규칙, 전투 동안 유지되는 패턴 기록.</summary>
    public class EncounterContractTests
    {
        sealed class Bag { public int Next; }

        [Test]
        public void RedAndBlue_CannotOverlap_YellowCanWithAnything()
        {
            Assert.IsFalse(ColorCombinationRules.CanOverlap(AttackColor.Red, AttackColor.Blue));
            Assert.IsFalse(ColorCombinationRules.CanOverlap(AttackColor.Blue, AttackColor.Red));
            Assert.IsTrue(ColorCombinationRules.CanOverlap(AttackColor.Yellow, AttackColor.Red));
            Assert.IsTrue(ColorCombinationRules.CanOverlap(AttackColor.Blue, AttackColor.Yellow));
            Assert.IsTrue(ColorCombinationRules.CanOverlap(AttackColor.Red, AttackColor.Red));

            Assert.IsTrue(ColorCombinationRules.IsAllowed(new[] { AttackColor.Yellow, AttackColor.Red, AttackColor.Red }));
            Assert.IsFalse(ColorCombinationRules.IsAllowed(new[] { AttackColor.Yellow, AttackColor.Red, AttackColor.Blue }));
            Assert.Greater(ColorCombinationRules.SwitchGraceSeconds, 0f);
        }

        [Test]
        public void EncounterMemory_KeepsStateAcrossContexts_OfSameBattle()
        {
            var memory = new EncounterMemory();
            var first = new PatternContext(null, null, null, null, 0f, 1, null, null, null, memory);
            first.Memory.GetOrCreate<Bag>("director").Next = 3;

            var nextTurn = new PatternContext(null, null, null, null, 0f, 1, null, null, null, memory);
            Assert.AreEqual(3, nextTurn.Memory.GetOrCreate<Bag>("director").Next, "다음 턴에도 유지");

            var other = new PatternContext(null, null, null, null, 0f, 1, null);
            Assert.AreEqual(0, other.Memory.GetOrCreate<Bag>("director").Next, "기록이 없으면 새로 시작");

            memory.Clear();
            Assert.AreEqual(0, memory.GetOrCreate<Bag>("director").Next);
        }
    }
}
