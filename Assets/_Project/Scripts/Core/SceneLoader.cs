using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game2Week.Core
{
    /// <summary>
    /// 씬 전환 창구. 로딩 중 중복 요청(확인키 연타 등)을 무시한다.
    /// 페이드 같은 전환 연출은 나중에 여기에 붙인다.
    /// </summary>
    public static class SceneLoader
    {
        public static bool IsLoading { get; private set; }

        public static bool Load(string sceneName)
        {
            if (IsLoading) return false;

            IsLoading = true;
            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                IsLoading = false;
                Debug.LogError($"씬을 불러올 수 없음 (빌드 목록 확인): {sceneName}");
                return false;
            }
            operation.completed += _ => IsLoading = false;
            return true;
        }

        // 도메인 리로드를 끈 플레이 모드에서도 정적 상태가 남지 않게
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => IsLoading = false;
    }
}
