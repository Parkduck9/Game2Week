using UnityEngine;

namespace Game2Week.Core
{
    /// <summary>
    /// 씬 배경음 (6단계 연결 지점). 씬마다 하나 — 클립을 넣으면 시작 때 서서히 커지고, 씬을 떠날 때 화면 페이드와 같이 줄어든다.
    /// 클립이 비어 있으면 아무 소리도 내지 않는다 (소리 출처는 사용자 결정). 음량은 설정의 배경음 값을 매 프레임 따른다.
    /// </summary>
    public sealed class SceneBgm : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] AudioRouting routing;
        [SerializeField] AudioClip clip;
        [SerializeField, Min(0f)] float fadeInSeconds = 0.6f;

        AudioSource source;
        float fade = 1f, fadeTarget = 1f, fadeSpeed;

        public AudioClip Clip => clip;
        public bool IsPlaying => source && source.isPlaying;
        public float SettingVolume => session && session.Save != null ? session.Save.Settings.bgmVolume : 1f;

        void Start()
        {
            SceneLoader.Leaving += OnLeaving;
            if (!clip) return;
            source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.ignoreListenerPause = true;
            fade = fadeInSeconds > 0f ? 0f : 1f;
            fadeSpeed = fadeInSeconds > 0f ? 1f / fadeInSeconds : 0f;
            fadeTarget = 1f;
            Apply();
            source.Play();
        }

        void OnDestroy() => SceneLoader.Leaving -= OnLeaving;

        void OnLeaving(float seconds)
        {
            fadeTarget = 0f;
            fadeSpeed = seconds > 0f ? 1f / seconds : 1000f;
        }

        void Update()
        {
            if (!source) return;
            fade = Mathf.MoveTowards(fade, fadeTarget, fadeSpeed * Time.unscaledDeltaTime);
            Apply();
        }

        void Apply()
        {
            float setting = SettingVolume;
            if (routing) routing.ApplyVolumes(setting, session && session.Save != null ? session.Save.Settings.sfxVolume : 1f);
            float volume = routing ? routing.Route(source, true, setting) : setting;
            source.volume = volume * fade;
        }
    }
}
