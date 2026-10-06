using UnityEngine;
using UnityEngine.Audio;

namespace Game2Week.Core
{
    /// <summary>
    /// 소리 출력 경로 (6단계). AudioMixer가 연결돼 있으면 배경음/효과음 그룹으로 보내고 음량을 dB로 설정한다.
    /// 믹서가 없으면 각 AudioSource 음량에 설정값(0~1)을 그대로 곱한다.
    /// (Unity는 스크립트로 AudioMixer 파일을 만들 수 없어 믹서는 에디터에서 만들어 여기 연결: Create ▸ Audio Mixer,
    ///  BGM·SFX 그룹 + 각 그룹 Volume을 노출 파라미터 "BgmVolume"·"SfxVolume"으로)
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Audio Routing", fileName = "AudioRouting")]
    public sealed class AudioRouting : ScriptableObject
    {
        public const float SilentDecibels = -80f;

        [SerializeField] AudioMixer mixer;
        [SerializeField] AudioMixerGroup bgmGroup;
        [SerializeField] AudioMixerGroup sfxGroup;
        [SerializeField] string bgmParameter = "BgmVolume";
        [SerializeField] string sfxParameter = "SfxVolume";

        public bool HasMixer => mixer;

        /// <summary>0~1 음량 → dB (0이면 무음 -80dB)</summary>
        public static float ToDecibels(float linear) =>
            linear <= 0.0001f ? SilentDecibels : Mathf.Max(SilentDecibels, 20f * Mathf.Log10(Mathf.Clamp01(linear)));

        /// <summary>믹서가 있으면 그룹 음량 설정. 없으면 아무것도 안 함 (소스 음량으로 처리).</summary>
        public void ApplyVolumes(float bgm, float sfx)
        {
            if (!mixer) return;
            mixer.SetFloat(bgmParameter, ToDecibels(bgm));
            mixer.SetFloat(sfxParameter, ToDecibels(sfx));
        }

        /// <summary>AudioSource를 그룹으로 보내고, 그 소스에 곱할 음량을 돌려준다 (믹서가 있으면 1 — 믹서가 처리).</summary>
        public float Route(AudioSource source, bool music, float settingVolume)
        {
            var group = music ? bgmGroup : sfxGroup;
            if (source && group) source.outputAudioMixerGroup = group;
            return mixer && group ? 1f : settingVolume;
        }
    }
}
