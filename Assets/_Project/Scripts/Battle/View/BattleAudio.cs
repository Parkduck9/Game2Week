using Game2Week.Battle.Patterns;
using Game2Week.Core;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 전투 효과음 연결 지점: 전투 이벤트(BattleEvents)·월드 사건(BattleFeedback) → 효과음.
    /// 소리 파일이 없어서 비어 있고, 클립만 넣으면 바로 울린다. 음량은 설정의 효과음 값 (AudioRouting에 믹서가 있으면 믹서로).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class BattleAudio : MonoBehaviour
    {
        [Header("전투 결과")]
        [SerializeField] AudioClip enemyHit;
        [SerializeField] AudioClip attackMiss;
        [SerializeField] AudioClip playerHurt;
        [SerializeField] AudioClip victory;
        [SerializeField] AudioClip defeat;
        [Header("주인공 동작")]
        [SerializeField] AudioClip dodge;
        [SerializeField] AudioClip jump;
        [SerializeField] AudioClip land;
        [SerializeField] AudioClip parry;
        [SerializeField] AudioClip brace;
        [SerializeField] AudioClip redPass;
        [SerializeField] AudioClip bluePass;
        [Header("공격")]
        [SerializeField] AudioClip bulletFired;
        [SerializeField] AudioClip warningStarted;
        [SerializeField] AudioRouting routing;

        AudioSource source;
        BattleEvents events;
        BattleFeedback feedback;
        System.Func<float> volume;

        /// <summary>마지막으로 울리려 한 소리 이름 (클립이 없어도 기록 — 테스트·디버그용)</summary>
        public string LastCue { get; private set; }
        public int CueCount { get; private set; }

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

        public void BindFeedback(BattleFeedback battleFeedback)
        {
            UnbindFeedback();
            feedback = battleFeedback;
            feedback.PlayerDodged += OnDodged;
            feedback.PlayerJumped += OnJumped;
            feedback.PlayerLanded += OnLanded;
            feedback.PlayerParried += OnParried;
            feedback.PlayerBraced += OnBraced;
            feedback.ColorPassed += OnColorPassed;
            feedback.BulletFired += OnBulletFired;
            feedback.WarningStarted += OnWarningStarted;
        }

        void OnDestroy() { Unbind(); UnbindFeedback(); }

        void Unbind()
        {
            if (events == null) return;
            events.EnemyDamaged -= OnEnemyDamaged;
            events.PlayerDamaged -= OnPlayerDamaged;
            events.BattleEnded -= OnBattleEnded;
            events = null;
        }

        void UnbindFeedback()
        {
            if (feedback == null) return;
            feedback.PlayerDodged -= OnDodged;
            feedback.PlayerJumped -= OnJumped;
            feedback.PlayerLanded -= OnLanded;
            feedback.PlayerParried -= OnParried;
            feedback.PlayerBraced -= OnBraced;
            feedback.ColorPassed -= OnColorPassed;
            feedback.BulletFired -= OnBulletFired;
            feedback.WarningStarted -= OnWarningStarted;
            feedback = null;
        }

        void OnEnemyDamaged(int amount) => Play(amount > 0 ? enemyHit : attackMiss, amount > 0 ? nameof(enemyHit) : nameof(attackMiss));
        void OnPlayerDamaged(int amount) => Play(playerHurt, nameof(playerHurt));
        void OnBattleEnded(BattleOutcome outcome) =>
            Play(outcome == BattleOutcome.PlayerDefeated ? defeat : victory, outcome == BattleOutcome.PlayerDefeated ? nameof(defeat) : nameof(victory));
        void OnDodged(Vector3 _) => Play(dodge, nameof(dodge));
        void OnJumped(Vector3 _) => Play(jump, nameof(jump));
        void OnLanded(Vector3 _) => Play(land, nameof(land));
        void OnParried(Vector3 _, float __) => Play(parry, nameof(parry));
        void OnBraced(Vector3 _) => Play(brace, nameof(brace));
        void OnColorPassed(AttackColor color, Vector3 _) =>
            Play(color == AttackColor.Red ? redPass : bluePass, color == AttackColor.Red ? nameof(redPass) : nameof(bluePass));
        void OnBulletFired(AttackColor _, Vector3 __) => Play(bulletFired, nameof(bulletFired));
        void OnWarningStarted(AttackColor _, Vector3 __) => Play(warningStarted, nameof(warningStarted));

        void Play(AudioClip clip, string cue)
        {
            LastCue = cue;
            CueCount++;
            if (!clip || !source) return;
            float setting = volume?.Invoke() ?? 1f;
            source.PlayOneShot(clip, routing ? routing.Route(source, false, setting) : setting);
        }
    }
}
