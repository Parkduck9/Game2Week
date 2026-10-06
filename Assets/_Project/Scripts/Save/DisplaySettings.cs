using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game2Week.Save
{
    /// <summary>화면 모드·해상도 적용. (음량은 사운드가 생기면 AudioMixer로 연결)</summary>
    public static class DisplaySettings
    {
        static readonly Vector2Int[] Standard16By9 = { new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(3840, 2160) };

        /// <summary>이 모니터가 지원하는 16:9 해상도 (작은 것부터). 알 수 없으면 표준 목록.</summary>
        public static IReadOnlyList<Vector2Int> Available16By9()
        {
            var supported = Screen.resolutions
                .Select(r => new Vector2Int(r.width, r.height))
                .Where(r => r.x * 9 == r.y * 16)
                .Distinct()
                .OrderBy(r => r.x)
                .ToList();
            return supported.Count > 0 ? supported : Standard16By9;
        }

        public static void Apply(SettingsData settings)
        {
            if (Application.isEditor) return; // 에디터 Game 뷰는 바꾸지 않음
            var mode = settings.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(settings.resolutionWidth, settings.resolutionHeight, mode);
        }
    }
}
