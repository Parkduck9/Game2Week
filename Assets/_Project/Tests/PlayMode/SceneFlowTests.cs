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

        static IEnumerator WaitForScene(string name)
        {
            float start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != name || SceneLoader.IsLoading)
            {
                if (Time.realtimeSinceStartup - start > Timeout) Assert.Fail($"씬 전환 시간 초과: {name}");
                yield return null;
            }
            yield return null; // Start() 실행 대기
        }

        static void Capture(string name) => SceneCapture.Save(name);

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
            Capture(SceneNames.MainMenu);

            Find<MainMenuController>().Choose(MainMenuController.StartIndex);
            yield return WaitForScene(SceneNames.Battle);
            Capture(SceneNames.Battle);

            Find<BattleFlowStub>().Finish(BattleOutcome.EnemySpared);
            yield return WaitForScene(SceneNames.Result);
            Assert.AreEqual("전투 종료", Find<ResultController>().Title);
            Capture(SceneNames.Result);

            Find<ResultController>().Choose(ResultController.RetryIndex);
            yield return WaitForScene(SceneNames.Battle);

            Find<BattleFlowStub>().Finish(BattleOutcome.PlayerDefeated);
            yield return WaitForScene(SceneNames.Result);
            Assert.AreEqual("GAME OVER", Find<ResultController>().Title);

            Find<ResultController>().Choose(ResultController.MainMenuIndex);
            yield return WaitForScene(SceneNames.MainMenu);
        }
    }
}
