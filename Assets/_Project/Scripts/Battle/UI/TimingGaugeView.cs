using System;
using Game2Week.Core;
using TMPro;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>
    /// 타이밍 공격 게이지 화면. 커서가 왼쪽→오른쪽으로 지나가고 확인키로 멈춘다.
    /// 판정은 TimingGauge(순수 로직)가 하고, 결과를 잠깐 보여 준 뒤 콜백.
    /// </summary>
    public sealed class TimingGaugeView : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] GameObject panel;
        [SerializeField] RectTransform cursor;
        [SerializeField] TMP_Text hint;
        [SerializeField, Min(0.3f)] float sweepSeconds = 1.3f;
        [Tooltip("멈춘 위치를 보여 주는 시간")]
        [SerializeField, Min(0f)] float resultHold = 0.4f;

        TimingGauge gauge;
        Action<float?> onFinished;
        float hold = -1f;
        bool subscribed;

        public bool IsOpen => panel.activeSelf;
        public float Position => gauge?.Position ?? 0f;

        void Awake() => panel.SetActive(false);

        void OnEnable()
        {
            if (gauge != null && !gauge.IsFinished) Subscribe(true);
        }

        void OnDisable() => Subscribe(false);

        public void Show(Action<float?> finished)
        {
            gauge = new TimingGauge(sweepSeconds);
            onFinished = finished;
            hold = -1f;
            panel.SetActive(true);
            hint.text = BattleTexts.GaugeHint;
            UpdateCursor();
            Subscribe(true);
        }

        public void Hide()
        {
            Subscribe(false);
            gauge = null;
            onFinished = null;
            panel.SetActive(false);
        }

        /// <summary>확인키와 같은 동작 (테스트용으로도 사용)</summary>
        public void Press()
        {
            if (gauge == null || gauge.IsFinished) return;
            gauge.Press();
            float a = gauge.Accuracy ?? 0f;
            hint.text = a >= 0.9f ? "완벽!" : a >= 0.6f ? "좋아!" : "아슬아슬";
            BeginHold();
        }

        void Update()
        {
            if (gauge == null) return;
            if (!gauge.IsFinished)
            {
                gauge.Advance(Time.deltaTime);
                UpdateCursor();
                if (gauge.IsFinished)
                {
                    hint.text = BattleTexts.Miss;
                    BeginHold();
                }
                return;
            }

            if (hold < 0f) return;
            hold -= Time.deltaTime;
            if (hold <= 0f) Finish();
        }

        void BeginHold()
        {
            Subscribe(false);
            hold = resultHold;
        }

        void Finish()
        {
            var callback = onFinished;
            var accuracy = gauge.Accuracy;
            Hide();
            callback?.Invoke(accuracy);
        }

        void UpdateCursor()
        {
            float x = gauge?.Position ?? 0f;
            cursor.anchorMin = new Vector2(x, 0f);
            cursor.anchorMax = new Vector2(x, 1f);
            cursor.anchoredPosition = Vector2.zero;
        }

        void Subscribe(bool on)
        {
            if (!input || on == subscribed) return;
            if (on) input.Submit += Press;
            else input.Submit -= Press;
            subscribed = on;
        }
    }
}
