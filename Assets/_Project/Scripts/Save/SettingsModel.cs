using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Save
{
    public enum SettingRow
    {
        BgmVolume,
        SfxVolume,
        DisplayMode,
        Resolution,
        TextSpeed,
        Back,
    }

    /// <summary>설정 화면 로직: 줄마다 좌우로 값을 바꾸고, 표시 문구를 만든다 (UI 없음).</summary>
    public sealed class SettingsModel
    {
        const float VolumeStep = 0.1f;

        readonly SettingsData data;
        readonly IReadOnlyList<Vector2Int> resolutions;

        public SettingsModel(SettingsData data, IReadOnlyList<Vector2Int> resolutions)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.resolutions = resolutions is { Count: > 0 } ? resolutions : new[] { new Vector2Int(data.resolutionWidth, data.resolutionHeight) };
        }

        public static readonly IReadOnlyList<SettingRow> Rows = (SettingRow[])Enum.GetValues(typeof(SettingRow));

        public SettingsData Data => data;

        /// <summary>화면에 바로 적용해야 하는 값(화면 모드·해상도)이 바뀌었으면 true를 돌려준다.</summary>
        public bool Change(SettingRow row, int delta, out bool displayChanged)
        {
            displayChanged = false;
            if (delta == 0) return false;
            switch (row)
            {
                case SettingRow.BgmVolume: return StepVolume(ref data.bgmVolume, delta);
                case SettingRow.SfxVolume: return StepVolume(ref data.sfxVolume, delta);
                case SettingRow.DisplayMode:
                    data.fullscreen = !data.fullscreen;
                    displayChanged = true;
                    return true;
                case SettingRow.Resolution:
                    int current = ResolutionIndex();
                    int next = Mathf.Clamp(current + Math.Sign(delta), 0, resolutions.Count - 1);
                    if (next == current) return false;
                    data.resolutionWidth = resolutions[next].x;
                    data.resolutionHeight = resolutions[next].y;
                    displayChanged = true;
                    return true;
                case SettingRow.TextSpeed:
                    int i = IndexOf(TextSpeeds.All, data.textSpeed);
                    int j = Mathf.Clamp(i + Math.Sign(delta), 0, TextSpeeds.All.Count - 1);
                    if (i == j) return false;
                    data.textSpeed = TextSpeeds.All[j];
                    return true;
                default:
                    return false;
            }
        }

        public string Label(SettingRow row) => row switch
        {
            SettingRow.BgmVolume => Row("배경음", $"{Mathf.RoundToInt(data.bgmVolume * 100)}%"),
            SettingRow.SfxVolume => Row("효과음", $"{Mathf.RoundToInt(data.sfxVolume * 100)}%"),
            SettingRow.DisplayMode => Row("화면 모드", data.fullscreen ? "전체 화면" : "창 모드"),
            SettingRow.Resolution => Row("해상도", $"{data.resolutionWidth} × {data.resolutionHeight}"),
            SettingRow.TextSpeed => Row("텍스트 속도", data.textSpeed switch { TextSpeeds.Slow => "느림", TextSpeeds.Fast => "빠름", _ => "보통" }),
            _ => "돌아가기",
        };

        // <pos>로 값 열을 맞추고, 꺾쇠는 태그로 읽히지 않게 noparse
        static string Row(string name, string value) => $"{name}<pos=45%><noparse><</noparse> {value} <noparse>></noparse>";

        int ResolutionIndex()
        {
            for (int i = 0; i < resolutions.Count; i++)
                if (resolutions[i].x == data.resolutionWidth && resolutions[i].y == data.resolutionHeight) return i;
            return resolutions.Count - 1;
        }

        static bool StepVolume(ref float value, int delta)
        {
            float next = Mathf.Clamp01(Mathf.Round((value + Math.Sign(delta) * VolumeStep) * 10f) / 10f);
            if (Mathf.Approximately(next, value)) return false;
            value = next;
            return true;
        }

        static int IndexOf(IReadOnlyList<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] == value) return i;
            return 1;
        }
    }
}
