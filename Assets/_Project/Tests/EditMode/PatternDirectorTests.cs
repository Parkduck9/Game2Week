using System.Collections.Generic;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Director;
using Game2Week.Data.Patterns;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEditor;

namespace Game2Week.Tests
{
    /// <summary>5단계: 턴별 패턴 선택(새 패턴 우선·셔플 백·색 겹침 금지), 8개 맵 누적 데이터, 색 안내.</summary>
    public class PatternDirectorTests
    {
        static DirectorState NewState(int seed = 7) => new() { Random = new System.Random(seed) };
        static readonly AttackColor Y = AttackColor.Yellow, R = AttackColor.Red, B = AttackColor.Blue;

        [Test]
        public void NewPattern_ComesFirstAndAlone_UntilIntroduced()
        {
            var s = NewState();
            var colors = new[] { Y, Y, Y };
            for (int i = 0; i < 3; i++)
            {
                var plan = DirectorPlanner.PlanTurn(s, colors, 2);
                Assert.AreEqual(0, plan.Primary, "소개 전에는 새 패턴 먼저");
                Assert.AreEqual(-1, plan.Secondary, "소개는 단독");
            }
            s.Introduced = true;
            var after = DirectorPlanner.PlanTurn(s, colors, 2);
            Assert.GreaterOrEqual(after.Primary, 0);
        }

        [Test]
        public void ShuffleBag_CoversAll_NoImmediateRepeat_AndIsSeeded()
        {
            var colors = new[] { Y, Y, Y, Y, Y };
            List<int> Run(int seed)
            {
                var s = NewState(seed); s.Introduced = true;
                return Enumerable.Range(0, 40).Select(_ => DirectorPlanner.PlanTurn(s, colors, 1).Primary).ToList();
            }
            var picks = Run(11);
            for (int i = 1; i < picks.Count; i++) Assert.AreNotEqual(picks[i - 1], picks[i], $"{i}번째 턴 연속 같은 패턴");
            CollectionAssert.IsSubsetOf(Enumerable.Range(0, 5).ToList(), picks.Take(10).ToList(), "두 바퀴 안에 모두 등장");
            CollectionAssert.AreEqual(picks, Run(11), "같은 시드 = 같은 순서");
        }

        [Test]
        public void Secondary_OnlyWhenAllowed_NeverRedWithBlue()
        {
            var colors = new[] { R, B, Y };
            var s = NewState(3); s.Introduced = true;
            for (int i = 0; i < 60; i++)
            {
                var plan = DirectorPlanner.PlanTurn(s, colors, 2);
                if (plan.Secondary < 0) continue;
                Assert.AreNotEqual(plan.Primary, plan.Secondary);
                Assert.IsTrue(ColorCombinationRules.CanOverlap(colors[plan.Primary], colors[plan.Secondary]),
                    $"{colors[plan.Primary]} + {colors[plan.Secondary]} 겹침 금지");
            }
            var single = NewState(); single.Introduced = true;
            Assert.AreEqual(-1, DirectorPlanner.PlanTurn(single, colors, 1).Secondary, "동시 상한 1이면 두 번째 없음");
        }

        [Test]
        public void EightStages_AddOnePatternEach_IntervalsShrink_NewColorWithKnownTrajectory()
        {
            var repo = new StageRepository(StageRepository.DefaultDirectory);
            var ids = repo.LoadIndex().stages;
            Assert.AreEqual(8, ids.Count);
            Assert.AreEqual("Pattern_YellowTraining", repo.LoadStage(ids[0]).Stage.enemyTurn.patterns.Single());

            float lastInterval = DifficultyProfile.BaseReferenceInterval + 0.01f;
            var seen = new HashSet<string> { "Pattern_YellowTraining" };
            for (int n = 2; n <= 8; n++)
            {
                Assert.AreEqual($"Attack_Stage_1-{n}", repo.LoadStage(ids[n - 1]).Stage.enemyTurn.patterns.Single());
                var e = AssetDatabase.LoadAssetAtPath<PatternEncounterData>($"Assets/_Project/Data/Patterns/Director/Encounter_1-{n}.asset");
                Assert.IsNotNull(e, $"1-{n} 구성");
                Assert.AreEqual(n, e.All().Count, $"1-{n}: {n}종 누적");
                CollectionAssert.IsSubsetOf(seen.ToList(), e.All().Select(p => p.name).ToList(), "앞에서 배운 패턴 모두 포함");
                Assert.IsFalse(seen.Contains(e.newPattern.name), "새 패턴은 처음 보는 것");
                seen.Add(e.newPattern.name);
                Assert.Less(e.profile.referenceInterval, lastInterval, "발사 간격은 단계마다 줄어든다");
                lastInterval = e.profile.referenceInterval;
                Assert.AreEqual(n >= 5 ? 2 : 1, e.profile.maxSimultaneous);
            }
            Assert.AreEqual(0.65f, lastInterval, 1e-4f);
        }

        [Test]
        public void ColorGuide_RedThenBlue_OncePerBattle_YellowIgnored()
        {
            var m = new ColorGuideModel();
            Assert.IsNull(m.Observe(new[] { Y, Y }));
            Assert.AreEqual(R, m.Observe(new[] { Y, R, B }), "빨강 먼저");
            Assert.AreEqual(B, m.Observe(new[] { Y, R, B }), "다음 프레임에 파랑");
            Assert.IsNull(m.Observe(new[] { R, B }), "같은 색은 한 번만");
            Assert.IsTrue(m.HasSeen(R) && m.HasSeen(B));
        }
    }
}
