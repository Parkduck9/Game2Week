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
    /// <summary>메인 → 전투 → 결과 → 메인 루프가 빌드 목록의 씬들로 끝까지 도는지 확인.</summary>
    public class SceneFlowTests
    {
        const float Timeout = 10f;

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

        static T Find<T>() where T : Object
        {
            var found = Object.FindAnyObjectByType<T>();
            Assert.IsNotNull(found, $"{typeof(T).Name} 없음");
            return found;
        }

        [UnityTest]
        public IEnumerator FullLoop_MainMenu_Battle_Result_Back()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return WaitForScene(SceneNames.MainMenu);
            SceneCapture.Save(SceneNames.MainMenu);

            Find<MainMenuController>().Choose(MainMenuController.StartIndex);
            yield return WaitForScene(SceneNames.Battle);
            BattleController battle = null;
            yield return WaitForBattle(c => battle = c);

            battle.Context.FinishBattle(BattleOutcome.EnemySpared);
            yield return WaitForScene(SceneNames.Result);
            Assert.AreEqual("전투 종료", Find<ResultController>().Title);
            SceneCapture.Save(SceneNames.Result);

            Find<ResultController>().Choose(ResultController.RetryIndex);
            yield return WaitForScene(SceneNames.Battle);
            yield return WaitForBattle(c => battle = c);

            battle.Context.FinishBattle(BattleOutcome.PlayerDefeated);
            yield return WaitForScene(SceneNames.Result);
            Assert.AreEqual("GAME OVER", Find<ResultController>().Title);

            Find<ResultController>().Choose(ResultController.MainMenuIndex);
            yield return WaitForScene(SceneNames.MainMenu);
        }
    }
}
