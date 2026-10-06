using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game2Week.Core
{
    /// <summary>
    /// 씬 전환 창구. 로딩 중 중복 요청(확인키 연타 등)을 무시한다.
    /// 전환 때 검은 화면으로 어두워졌다가(FadeSeconds) 새 씬에서 다시 밝아진다. 배경음은 Leaving을 듣고 같이 줄어든다.
    /// </summary>
    public static class SceneLoader
    {
        public const float DefaultFadeSeconds = 0.25f;

        public static bool IsLoading { get; private set; }
        /// <summary>0이면 페이드 없이 바로 전환</summary>
        public static float FadeSeconds { get; set; } = DefaultFadeSeconds;
        /// <summary>전환 시작 (인자: 페이드 시간) — 배경음 줄이기용</summary>
        public static event Action<float> Leaving;

        static ScreenFader fader;
        public static ScreenFader Fader => fader;

        public static bool Load(string sceneName)
        {
            if (IsLoading) return false;
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"씬을 불러올 수 없음 (빌드 목록 확인): {sceneName}");
                return false;
            }

            IsLoading = true;
            Leaving?.Invoke(FadeSeconds);
            if (FadeSeconds <= 0f || !Application.isPlaying) { StartLoad(sceneName); return true; }
            if (!fader) fader = ScreenFader.Create();
            fader.StartCoroutine(FadeAndLoad(sceneName));
            return true;
        }

        static IEnumerator FadeAndLoad(string sceneName)
        {
            yield return fader.FadeTo(1f, FadeSeconds);
            var operation = StartLoad(sceneName);
            while (operation != null && !operation.isDone) yield return null;
            yield return fader.FadeTo(0f, FadeSeconds);
        }

        static AsyncOperation StartLoad(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                IsLoading = false;
                Debug.LogError($"씬을 불러올 수 없음 (빌드 목록 확인): {sceneName}");
                return null;
            }
            operation.completed += _ => IsLoading = false;
            return operation;
        }

        // 도메인 리로드를 끈 플레이 모드에서도 정적 상태가 남지 않게
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            IsLoading = false;
            FadeSeconds = DefaultFadeSeconds;
            Leaving = null;
            fader = null;
        }
    }
}
