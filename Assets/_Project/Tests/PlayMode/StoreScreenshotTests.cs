using System;
using System.Collections;
using Game2Week.Battle;
using Game2Week.Core;
using Game2Week.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>
    /// 스토어 제출용 스크린샷 (1920×1080, 데스크톱 최소 1366×768) — 실제 게임 화면 그대로, 합성 없음.
    /// 수 초~수십 초 걸려 명령줄에 -storeShots가 있을 때만: 결과 Logs/scene_store_*.png → Reference/store/screenshot_v2_*.png로 복사.
    /// </summary>
    public class StoreScreenshotTests
    {
        const int W = 1920, H = 1080;

        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() { Time.timeScale = 1f; TestSave.End(); }

        static void Shot(string name) => SceneCapture.Save("store_" + name, W, H);

        [UnityTest]
        public IEnumerator CaptureStoreScreenshots()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-storeShots") < 0) Assert.Ignore("스토어 스크린샷은 -storeShots로 실행");

            // 1. 제목 화면
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return SceneFlowTests.WaitForScene(SceneNames.MainMenu);
            yield return new WaitForSeconds(1.2f);
            Shot("1_title");

            // 2. 공격 순간 (1-1) — 타격 연출
            BattleController battle = null;
            yield return SceneFlowTests.EnterStage(0, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            BattleTestUtil.MoveNearEnemy(battle, 0.8f);
            battle.Context.ChangeState(BattleStateId.ActionMenu);
            yield return null;
            battle.Context.PlayPresentation(ActionCue.Strike, () => battle.Context.Events.RaiseEnemyDamaged(battle.Context.Enemy.TakeDamage(12)), () => { });
            yield return new WaitForSeconds(0.34f);
            Shot("2_strike");
            yield return new WaitForSeconds(1f);

            // 3. 탄막 회피 — 1-4 (빨강 포함), 적과 9m 거리
            yield return SceneFlowTests.EnterStage(3, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            BattleTestUtil.MoveNearEnemy(battle, 9f);
            yield return new WaitForSeconds(4.5f);
            Shot("3_dodge");

            // 4. 후반 맵 겹 공격 — 1-8
            yield return SceneFlowTests.EnterStage(7, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            BattleTestUtil.MoveNearEnemy(battle, 8f);
            yield return new WaitForSeconds(5f);
            Shot("4_layers");

            // 5. 적에게 닿은 뒤 행동 메뉴 — 1-2
            yield return SceneFlowTests.EnterStage(1, c => battle = c);
            battle.Context.ChangeState(BattleStateId.EnemyTurn);
            yield return null;
            BattleTestUtil.MoveNearEnemy(battle, 1.2f);
            battle.Context.ChangeState(BattleStateId.ActionMenu);
            yield return new WaitForSeconds(1f);
            Shot("5_action_menu");
        }
    }
}
