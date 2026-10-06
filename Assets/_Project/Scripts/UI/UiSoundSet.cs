using Game2Week.Core;
using UnityEngine;

namespace Game2Week.UI
{
    /// <summary>메뉴 효과음 묶음 (6단계 연결 지점). 클립이 비어 있으면 무음. 음량은 설정의 효과음 값.</summary>
    [CreateAssetMenu(menuName = "Game2Week/UI Sound Set", fileName = "UiSounds")]
    public sealed class UiSoundSet : ScriptableObject
    {
        [SerializeField] GameSession session;
        [SerializeField] AudioRouting routing;
        public AudioClip move;
        public AudioClip submit;
        public AudioClip cancel;
        [Tooltip("비활성 항목을 고르려 할 때")]
        public AudioClip denied;

        public float SfxVolume => session && session.Save != null ? session.Save.Settings.sfxVolume : 1f;

        public void Play(AudioSource source, AudioClip clip)
        {
            if (!clip || !source) return;
            float volume = routing ? routing.Route(source, false, SfxVolume) : SfxVolume;
            source.PlayOneShot(clip, volume);
        }
    }
}
