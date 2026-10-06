using System;
using TMPro;
using UnityEngine;

namespace Game2Week.UI
{
    /// <summary>예/아니요 확인 창. 여는 쪽은 자기 메뉴를 꺼 둔다 (입력이 겹치지 않게).</summary>
    public sealed class ConfirmPopup : MonoBehaviour
    {
        public const int YesIndex = 0;
        public const int NoIndex = 1;

        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text message;
        [SerializeField] MenuNavigator menu;

        Action onYes;
        Action onNo;

        public bool IsOpen => panel.activeSelf;
        public string Message => message.text;

        void Awake()
        {
            menu.Selected += OnSelected;
            menu.Cancelled += OnCancelled;
            panel.SetActive(false);
        }

        void OnDestroy()
        {
            menu.Selected -= OnSelected;
            menu.Cancelled -= OnCancelled;
        }

        /// <summary>되돌릴 수 없는 일이면 defaultNo = true (커서가 "아니요"에서 시작).</summary>
        public void Show(string text, Action yes, Action no, bool defaultNo = true)
        {
            message.text = text;
            onYes = yes;
            onNo = no;
            panel.SetActive(true);
            menu.SetItems(UiTexts.YesNo);
            if (defaultNo) menu.Select(NoIndex);
        }

        public void Choose(int index) => OnSelected(index);

        void OnSelected(int index)
        {
            var callback = index == YesIndex ? onYes : onNo;
            Close();
            callback?.Invoke();
        }

        void OnCancelled()
        {
            var callback = onNo;
            Close();
            callback?.Invoke();
        }

        void Close()
        {
            onYes = onNo = null;
            panel.SetActive(false);
        }
    }
}
