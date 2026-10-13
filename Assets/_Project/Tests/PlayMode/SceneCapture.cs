using System.IO;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>
    /// 확인용 화면 캡처 → Logs/scene_<name>.png. 오버레이 캔버스는 잠시 카메라 캔버스로 바꿔 함께 찍는다.
    /// 배치모드 첫 렌더는 SRP Batcher 머티리얼이 덜 올라가 색이 깨질 수 있어 두 번 렌더한다.
    /// </summary>
    public static class SceneCapture
    {
        public static void Save(string name) => Save(name, 1280, 720);

        /// <summary>크기 지정 캡처 (스토어 스크린샷은 1920×1080 — 데스크톱 최소 1366×768)</summary>
        public static void Save(string name, int width, int height)
        {
            var cam = Camera.main;
            if (cam == null) return;

            var canvas = Object.FindAnyObjectByType<Canvas>();
            var mode = canvas ? canvas.renderMode : RenderMode.ScreenSpaceOverlay;
            if (canvas)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = cam.nearClipPlane + 0.05f;
                Canvas.ForceUpdateCanvases();
            }

            var rt = new RenderTexture(width, height, 24) { antiAliasing = 4 };
            var previousTarget=cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath, $"../Logs/scene_{name}.png"), tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = previousTarget;
            Object.Destroy(rt);
            Object.Destroy(tex);
            if (canvas) canvas.renderMode = mode;
        }
    }
}
