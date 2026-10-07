using System.Collections.Generic;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Director;
using Game2Week.Data;
using Game2Week.Data.Patterns;
using Game2Week.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>5단계: 턴별 패턴 선택(새 패턴 우선·셔플 백·색 겹침 금지), 8개 맵 누적 데이터, 색 안내.</summary>
    public class PatternDirectorTests
    {
        static DirectorState NewState(int seed = 7) => new() { Random = new System.Random(seed) };
        static readonly AttackColor Y = AttackColor.Yellow, R = AttackColor.Red, B = AttackColor.Blue;

        [Test]
        public void NewPattern_IsPrimary_InIntroTurns_OtherLayersJoinLater()
        {
            var s = NewState();
            var colors = new[] { Y, Y, Y };
            for (int i = 0; i < 3; i++)
            {
                var plan = DirectorPlanner.PlanTurn(s, colors, 2);
                Assert.AreEqual(0, plan.Primary, "소개 전에는 새 패턴이 주 공격");
                Assert.IsTrue(plan.IsIntroTurn, "소개 턴 — 다른 겹은 소개 뒤에 합류 (9단계)");
                Assert.AreEqual(2, plan.LayerCount);
            }
            s.Introduced = true;
            var after = DirectorPlanner.PlanTurn(s, colors, 2);
            Assert.GreaterOrEqual(after.Primary, 0);
            Assert.IsFalse(after.IsIntroTurn);
            var lone = NewState();
            Assert.IsFalse(DirectorPlanner.PlanTurn(lone, new[] { Y }, 3).IsIntroTurn, "패턴이 하나면 소개 턴 구분 없음");
        }

        [Test]
        public void ThreeLayers_AllPairsColorCompatible_AndDistinct()
        {
            var colors = new[] { Y, R, B, Y, Y };
            var s = NewState(5); s.Introduced = true;
            for (int i = 0; i < 80; i++)
            {
                var plan = DirectorPlanner.PlanTurn(s, colors, 3);
                Assert.LessOrEqual(plan.LayerCount, 3);
                CollectionAssert.AllItemsAreUnique(plan.Layers);
                for (int a = 0; a < plan.LayerCount; a++)
                    for (int b = a + 1; b < plan.LayerCount; b++)
                        Assert.IsTrue(ColorCombinationRules.CanOverlap(colors[plan.Layers[a]], colors[plan.Layers[b]]), "빨강·파랑 동시 금지");
            }
            var yellow = NewState(); yellow.Introduced = true;
            Assert.AreEqual(3, DirectorPlanner.PlanTurn(yellow, new[] { Y, Y, Y, Y }, 3).LayerCount, "가능하면 3겹 모두");
        }

        [Test]
        public void Profile_Phases_AddLayers_AndScale()
        {
            var p = ScriptableObject.CreateInstance<DifficultyProfile>();
            try
            {
                p.referenceInterval = 1.4f; p.speedScale = 1f; p.maxSimultaneous = 2;
                p.phases = new List<DifficultyPhase>
                {
                    new() { hpBelow = 0.6f, extraLayers = 1, intervalMultiplier = 0.9f, speedMultiplier = 1.1f },
                    new() { hpBelow = 0.3f, extraLayers = 1, intervalMultiplier = 0.5f, speedMultiplier = 1f },
                };
                Assert.AreEqual(0, p.PhaseFor(1f));
                Assert.AreEqual(1, p.PhaseFor(0.6f));
                Assert.AreEqual(2, p.PhaseFor(0.1f));
                Assert.AreEqual(2, p.LayersFor(0));
                Assert.AreEqual(3, p.LayersFor(1));
                Assert.AreEqual(DifficultyProfile.MaxLayers, p.LayersFor(2), "상한 3");
                var (interval, speed) = p.ScalesFor(2);
                Assert.AreEqual(0.45f, interval, 1e-4f);
                Assert.AreEqual(1.1f, speed, 1e-4f);
            }
            finally { Object.DestroyImmediate(p); }
        }

        [Test]
        public void Candidates_AddEnemyMoves_AndPhaseMovesOnlyWhenHurt()
        {
            var e = AssetDatabase.LoadAssetAtPath<PatternEncounterData>("Assets/_Project/Data/Patterns/Director/Encounter_1-2.asset");
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Enemy_Test2.asset");
            var info = new EnemyPatternInfo(enemy.SignatureMoves, enemy.PhaseMoves, () => 1f);
            var calm = PatternDirector.BuildCandidates(e, info, 0);
            Assert.AreSame(e.newPattern, calm[0], "0번은 맵의 새 패턴 그대로");
            CollectionAssert.IsSubsetOf(enemy.SignatureMoves, calm);
            CollectionAssert.IsNotSubsetOf(enemy.PhaseMoves, calm);
            CollectionAssert.IsSubsetOf(enemy.PhaseMoves, PatternDirector.BuildCandidates(e, info, 1));
            CollectionAssert.AllItemsAreUnique(PatternDirector.BuildCandidates(e, info, 1));
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

            float lastInterval = DifficultyProfile.BaseReferenceInterval + 0.01f;
            var seen = new HashSet<string>();
            for (int n = 1; n <= 8; n++)
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
                Assert.AreEqual(n >= 7 ? 3 : 2, e.profile.maxSimultaneous, "9단계: 처음부터 2겹, 7·8은 3겹");
                Assert.Greater(e.profile.phases.Count, 0, "체력 단계");
            }
            Assert.AreEqual("Pattern_YellowTraining", AssetDatabase.LoadAssetAtPath<PatternEncounterData>("Assets/_Project/Data/Patterns/Director/Encounter_1-1.asset").newPattern.name);
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
