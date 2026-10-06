using System.Collections;
using Game2Week.Battle;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.Patterns.Director;
using Game2Week.Battle.UI;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>5단계 실제 전투: 새 패턴 단독 소개 → 턴 넘어 기록 유지 → 두 패턴 겹침(색 규칙) → 정리, 색 처음 등장 안내.</summary>
    public class PatternDirectorRuntimeTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() { Time.timeScale = 1f; TestSave.End(); }

        static IEnumerator NextEnemyTurn(BattleController battle)
        {
            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return null;
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Stage5_IntroducesAlone_KeepsMemory_ThenOverlapsCompatibleColors()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(4, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;

            var director = (PatternDirector)battle.World.Patterns.Current;
            Assert.AreEqual(0, director.Plan.Primary, "첫 턴은 새 패턴(포물선)");
            Assert.AreEqual(-1, director.Plan.Secondary);
            Assert.AreEqual(1, director.ActiveObjects.Count);
            var state = director.State;
            yield return new WaitForSeconds(PatternDirector.IntroduceSeconds + 0.2f);
            Assert.IsTrue(state.Introduced);

            // 두 번째 패턴이 계획된 턴이 나올 때까지 (셔플 백, 동시 상한 2)
            for (int turn = 0; turn < 8; turn++)
            {
                yield return NextEnemyTurn(battle);
                director = (PatternDirector)battle.World.Patterns.Current;
                Assert.AreSame(state, director.State, "턴이 바뀌어도 같은 전투 기록");
                if (director.Plan.Secondary >= 0) break;
            }
            Assert.GreaterOrEqual(director.Plan.Secondary, 0, "8턴 안에 겹치는 턴이 있어야 함");
            var patterns = director.Encounter.All();
            Assert.IsTrue(ColorCombinationRules.CanOverlap(
                PatternDirector.ColorOf(patterns[director.Plan.Primary]), PatternDirector.ColorOf(patterns[director.Plan.Secondary])));

            yield return new WaitForSeconds(director.Encounter.profile.secondPatternDelay + 0.3f);
            Assert.AreEqual(2, director.ActiveObjects.Count, "두 번째 패턴이 늦게 겹침");
            SceneCapture.Save("director_stage5_overlap");

            var children = new System.Collections.Generic.List<GameObject>(director.ActiveObjects);
            battle.Context.ChangeState(BattleStateId.ItemMenu);
            yield return null;
            Assert.IsNull(battle.World.Patterns.CurrentObject);
            foreach (var go in children) Assert.IsTrue(go == null, "턴이 끝나면 하위 패턴 정리");
        }

        [UnityTest]
        public IEnumerator Stage4_RedGuide_ShowsOnce()
        {
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(3, c => battle = c);
            var guide = Object.FindAnyObjectByType<ColorGuideView>(FindObjectsInactive.Include);
            Assert.IsNotNull(guide, "Battle 씬에 색 안내 연결");
            Assert.IsFalse(guide.IsShowing);

            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(guide.IsShowing, "새 패턴(빨강) 예고와 함께 안내");
            Assert.AreEqual(BattleTexts.RedGuide, guide.Text);
            SceneCapture.Save("director_red_guide");

            yield return new WaitForSeconds(3.2f);
            Assert.IsFalse(guide.IsShowing, "잠깐 뒤 사라짐");
            yield return NextEnemyTurn(battle);
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(guide.IsShowing, "같은 전투에서 같은 색은 한 번만");
        }
    }
}
