using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 사운드 연결 지점: 전투 이벤트 → 효과음. 지금은 소리 파일이 없어서 비어 있고,
    /// 클립만 넣으면 바로 울린다. 음량은 설정의 효과음 값을 따른다.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class BattleAudio : MonoBehaviour
    {
        [SerializeField] AudioClip enemyHit;
        [SerializeField] AudioClip attackMiss;
        [SerializeField] AudioClip playerHurt;
        [SerializeField] AudioClip victory;
        [SerializeField] AudioClip defeat;

        AudioSource source;
        BattleEvents events;
        System.Func<float> volume;

        public void Bind(BattleEvents battleEvents, System.Func<float> sfxVolume)
        {
            Unbind();
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            events = battleEvents;
            volume = sfxVolume;
            events.EnemyDamaged += OnEnemyDamaged;
            events.PlayerDamaged += OnPlayerDamaged;
            events.BattleEnded += OnBattleEnded;
        }

        void OnDestroy() => Unbind();

        void Unbind()
        {
            if (events == null) return;
            events.EnemyDamaged -= OnEnemyDamaged;
            events.PlayerDamaged -= OnPlayerDamaged;
            events.BattleEnded -= OnBattleEnded;
            events = null;
        }

        void OnEnemyDamaged(int amount) => Play(amount > 0 ? enemyHit : attackMiss);

        void OnPlayerDamaged(int amount) => Play(playerHurt);

        void OnBattleEnded(BattleOutcome outcome) => Play(outcome == BattleOutcome.PlayerDefeated ? defeat : victory);

        void Play(AudioClip clip)
        {
            if (!clip || !source) return;
            source.PlayOneShot(clip, volume?.Invoke() ?? 1f);
        }
    }
}
