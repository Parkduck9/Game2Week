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

        // 확인용 화면 캡처 → Logs/scene_<name>.png (오버레이 캔버스를 잠시 카메라 캔버스로 바꿔 렌더)
        static void Capture(string name)
        {
            var cam = Camera.main;
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (cam == null || canvas == null) return;

            var mode = canvas.renderMode;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1f;
            Canvas.ForceUpdateCanvases();

            var rt = new RenderTexture(1280, 720, 24) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.Render();
            cam.Render(); // 배치모드 첫 렌더는 SRP Batcher 머티리얼 데이터가 덜 올라가 있을 수 있어 두 번째 프레임을 쓴다
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(Application.dataPath, $"../Logs/scene_{name}.png"), tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.Destroy(rt);
            Object.Destroy(tex);
            canvas.renderMode = mode;
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
