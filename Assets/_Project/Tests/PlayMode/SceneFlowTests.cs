using System.Collections;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Core;
using Game2Week.Flow;
using Game2Week.Save;
using Game2Week.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    /// <summary>서비스 흐름: 메인 → 스테이지 선택 → 전투 → 결과 → 엔딩/선택/메인, 진행·설정 저장 유지.</summary>
    public class SceneFlowTests
    {
        const float Timeout = 10f;

        [SetUp]
        public void SetUp() => TestSave.Begin();

        [TearDown]
        public void TearDown() => TestSave.End();

        public static IEnumerator WaitForScene(string name)
        {
            float start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != name || SceneLoader.IsLoading)
            {
                if (Time.realtimeSinceStartup - start > Timeout) Assert.Fail($"씬 전환 시간 초과: {name}");
                yield return null;
            }
            yield return null; // Start() 실행 대기
        }

        public static IEnumerator WaitForBattle(System.Action<BattleController> found)
        {
            float start = Time.realtimeSinceStartup;
            BattleController controller = null;
            while (controller == null || !controller.IsReady)
            {
                if (Time.realtimeSinceStartup - start > Timeout) Assert.Fail("Battle 준비 시간 초과");
                controller = Object.FindAnyObjectByType<BattleController>();
                yield return null;
            }
            found(controller);
        }

        /// <summary>
        /// 스테이지 선택 화면을 거쳐 index번 스테이지 전투로 들어간다.
        /// Battle 씬을 바로 열면 이전 테스트의 스테이지가 남아 있을 수 있어 항상 이걸로 들어간다.
        /// </summary>
        public static IEnumerator EnterStage(int index, System.Action<BattleController> found)
        {
            SceneManager.LoadScene(SceneNames.StageSelect);
            yield return WaitForScene(SceneNames.StageSelect);
            Object.FindAnyObjectByType<StageSelectController>().Choose(index);
            yield return WaitForScene(SceneNames.Battle);
            yield return WaitForBattle(found);
        }

        static T Find<T>() where T : Object
        {
            var found = Object.FindAnyObjectByType<T>();
            Assert.IsNotNull(found, $"{typeof(T).Name} 없음");
            return found;
        }

        static IEnumerator PlayStageOne(BattleOutcome outcome)
        {
            Find<StageSelectController>().Choose(0);
            yield return WaitForScene(SceneNames.Battle);
            BattleController battle = null;
            yield return WaitForBattle(c => battle = c);
            battle.Context.FinishBattle(outcome);
            yield return WaitForScene(SceneNames.Result);
        }

        [UnityTest]
        public IEnumerator NewGame_Play_Records_Continue_Settings()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return WaitForScene(SceneNames.MainMenu);
            var main = Find<MainMenuController>();
            Assert.IsFalse(main.CanContinue);
            Assert.IsFalse(main.Menu.IsEnabled(MainMenuController.ContinueIndex), "세이브 없으면 이어하기 비활성");
            SceneCapture.Save("main_fresh");

            main.Choose(MainMenuController.NewGameIndex); // 진행 없음 → 확인 없이
            yield return WaitForScene(SceneNames.StageSelect);
            Assert.AreEqual(1, Find<StageSelectController>().Items.Count, "처음엔 1번만");

            yield return PlayStageOne(BattleOutcome.EnemySpared);
            var result = Find<ResultController>();
            Assert.AreEqual("전투 종료", result.Title);
            StringAssert.Contains(UiTexts.NewRecord, result.TimeText);
            Assert.AreEqual(UiTexts.NextStage, result.Items[0], "1-1 클리어 → 다음 스테이지(1-2) 해금");
            SceneCapture.Save("result_victory");

            result.Choose(UiTexts.Retry);
            yield return WaitForScene(SceneNames.Battle);
            BattleController battle = null;
            yield return WaitForBattle(c => battle = c);
            battle.Context.FinishBattle(BattleOutcome.PlayerDefeated);
            yield return WaitForScene(SceneNames.Result);
            result = Find<ResultController>();
            Assert.AreEqual("GAME OVER", result.Title);
            CollectionAssert.AreEqual(new[] { UiTexts.Retry, UiTexts.StageSelect }, result.Items);

            result.Choose(UiTexts.StageSelect);
            yield return WaitForScene(SceneNames.StageSelect);
            StringAssert.Contains("최고", Find<StageSelectController>().Items[0]);
            Assert.AreEqual(2, Find<StageSelectController>().Items.Count, "1-2가 열림");
            StringAssert.Contains(UiTexts.NoRecord, Find<StageSelectController>().Items[1]);
            SceneCapture.Save("stage_select");

            Find<StageSelectController>().Back();
            yield return WaitForScene(SceneNames.MainMenu);
            main = Find<MainMenuController>();
            Assert.IsTrue(main.CanContinue, "클리어 후 이어하기 가능");

            // 새로 시작 → 확인 창 → 아니요: 진행 유지
            main.Choose(MainMenuController.NewGameIndex);
            Assert.IsTrue(main.Confirm.IsOpen);
            SceneCapture.Save("main_confirm");
            main.Confirm.Choose(ConfirmPopup.NoIndex);
            Assert.IsTrue(main.CanContinue);

            // 설정 → 텍스트 속도 빠름 → 닫으면 파일에 저장
            main.Choose(MainMenuController.SettingsIndex);
            Assert.IsTrue(main.Settings.IsOpen);
            main.Settings.Change(SettingRow.TextSpeed, 1);
            SceneCapture.Save("settings");
            main.Settings.Close();
            Assert.AreEqual(TextSpeeds.Fast, new SaveService(TestSave.Directory).Settings.textSpeed);
            Assert.IsTrue(new SaveService(TestSave.Directory).HasProgress, "진행도 파일에 남아 있음");
        }

        [UnityTest]
        public IEnumerator LastStageClear_ShowsEnding_ThenMain()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return WaitForScene(SceneNames.MainMenu);
            Find<MainMenuController>().Choose(MainMenuController.NewGameIndex);
            yield return WaitForScene(SceneNames.StageSelect);

            yield return PlayStageOne(BattleOutcome.EnemyDefeated);
            Find<ResultController>().Choose(UiTexts.NextStage);
            yield return WaitForScene(SceneNames.Battle);
            BattleController battle = null;
            yield return WaitForBattle(c => battle = c);
            Assert.AreEqual("stage_002", battle.Context.Stage.id, "다음 스테이지 = 2번");

            // 스테이지 수와 상관없이: 마지막 스테이지로 바로 가서 클리어
            Find<PauseMenu>().CanPause = false;
            SceneLoader.Load(SceneNames.StageSelect);
            yield return WaitForScene(SceneNames.StageSelect);
            var select = Find<StageSelectController>();
            int last = Find<StageSelectController>().Items.Count; // 해금된 수 (2) — 마지막은 Choose로 직접
            Assert.AreEqual(2, last);
            var stageIds = new Game2Week.Stages.StageRepository(Game2Week.Stages.StageRepository.DefaultDirectory).LoadIndex().stages;
            select.Choose(stageIds.Count - 1);
            yield return WaitForScene(SceneNames.Battle);
            yield return WaitForBattle(c => battle = c);
            Assert.AreEqual(stageIds[stageIds.Count - 1], battle.Context.Stage.id);
            battle.Context.FinishBattle(BattleOutcome.EnemySpared);
            yield return WaitForScene(SceneNames.Result);

            Assert.AreEqual(UiTexts.ToEnding, Find<ResultController>().Items[0], "마지막 스테이지 첫 클리어 → 엔딩으로");
            Find<ResultController>().Choose(UiTexts.ToEnding);
            yield return WaitForScene(SceneNames.Ending);
            yield return new WaitForSeconds(2f);
            SceneCapture.Save("ending");
            Assert.IsTrue(new SaveService(TestSave.Directory).Progress.endingSeen);

            Find<EndingController>().Finish();
            yield return WaitForScene(SceneNames.MainMenu);
            Assert.IsTrue(Find<MainMenuController>().CanContinue);
        }
    }
}
