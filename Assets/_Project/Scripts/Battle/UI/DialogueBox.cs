using System;
using Game2Week.Core;
using TMPro;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>
    /// 화면 하단 대사창. 한 글자씩 출력하고, 확인키: 출력 중이면 전부 표시 → 다 나왔으면 닫기.
    /// </summary>
    public sealed class DialogueBox : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text text;
        [SerializeField, Min(1f)] float charactersPerSecond = 45f;

        Action onClosed;
        float visible;
        int total;
        bool typing;
        bool subscribed;

        public bool IsWaitingForInput { get; private set; }
        public string Text => text.text;

        /// <summary>설정의 텍스트 속도 배율 (BattleController가 연결)</summary>
        public System.Func<float> SpeedMultiplier { get; set; }

        /// <summary>대사를 출력하고 확인키로 닫히면 onClosed.</summary>
        public void Show(string message, Action closed)
        {
            panel.SetActive(true);
            SetText(message);
            text.maxVisibleCharacters = 0;
            visible = 0f;
            typing = total > 0;
            onClosed = closed;
            IsWaitingForInput = true;
            Subscribe(true);
        }

        /// <summary>기다리지 않고 글만 띄운다.</summary>
        public void ShowStatic(string message)
        {
            Subscribe(false);
            IsWaitingForInput = false;
            onClosed = null;
            panel.SetActive(true);
            SetText(message);
            text.maxVisibleCharacters = int.MaxValue;
            typing = false;
        }

        public void Hide()
        {
            Subscribe(false);
            IsWaitingForInput = false;
            onClosed = null;
            panel.SetActive(false);
        }

        /// <summary>확인키와 같은 동작 (테스트용으로도 사용).</summary>
        public void Advance()
        {
            if (!IsWaitingForInput) return;
            if (typing)
            {
                typing = false;
                text.maxVisibleCharacters = int.MaxValue;
                return;
            }
            IsWaitingForInput = false;
            Subscribe(false);
            var callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }

        void SetText(string message)
        {
            text.text = message;
            text.ForceMeshUpdate();
            total = text.textInfo.characterCount;
        }

        void Update()
        {
            if (!typing) return;
            visible += charactersPerSecond * (SpeedMultiplier?.Invoke() ?? 1f) * Time.deltaTime;
            text.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(visible));
            if (visible >= total)
            {
                typing = false;
                text.maxVisibleCharacters = int.MaxValue;
            }
        }

        // 일시정지로 꺼졌다 켜지면 기다리던 대사의 입력을 다시 받는다
        void OnEnable()
        {
            if (IsWaitingForInput) Subscribe(true);
        }

        void OnDisable() => Subscribe(false);

        void Subscribe(bool on)
        {
            if (!input || on == subscribed) return;
            if (on) input.Submit += Advance;
            else input.Submit -= Advance;
            subscribed = on;
        }
    }
}
