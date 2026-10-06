using System;
using System.Collections.Generic;
using Game2Week.UI;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>IBattleUi 구현 — 대사창, 가로 메뉴, 목록 메뉴, 탄막 턴 표시, 팝업을 묶는다.</summary>
    public sealed class BattleUi : MonoBehaviour, IBattleUi
    {
        [SerializeField] DialogueBox dialogue;
        [Tooltip("가로 버튼 메뉴 (공격/행동/자비, 아이템/넘기기)")]
        [SerializeField] MenuNavigator mainMenu;
        [Tooltip("대사창 안 세로 목록 (행동·아이템 목록)")]
        [SerializeField] MenuNavigator listMenu;
        [SerializeField] TurnHud turnHud;
        [SerializeField] PopupText popup;

        Action<int> mainSelected;
        Action<int> listSelected;
        Action listCancelled;

        public DialogueBox Dialogue => dialogue;
        public bool IsMainMenuOpen => mainMenu.gameObject.activeSelf;
        public bool IsListMenuOpen => listMenu.gameObject.activeSelf;

        void Awake()
        {
            mainMenu.Selected += OnMainSelected;
            listMenu.Selected += OnListSelected;
            listMenu.Cancelled += OnListCancelled;
            HideAll();
        }

        void OnDestroy()
        {
            mainMenu.Selected -= OnMainSelected;
            listMenu.Selected -= OnListSelected;
            listMenu.Cancelled -= OnListCancelled;
        }

        public void ShowDialogue(string text, Action onClosed)
        {
            HideMenus();
            dialogue.Show(text, onClosed);
        }

        public void ShowBoxText(string text) => dialogue.ShowStatic(text);

        public void ShowMainMenu(IReadOnlyList<string> items, Action<int> onSelected)
        {
            mainSelected = onSelected;
            mainMenu.gameObject.SetActive(true);
            mainMenu.SetItems(items);
        }

        public void ShowListMenu(IReadOnlyList<string> items, Action<int> onSelected, Action onCancel)
        {
            dialogue.ShowStatic(string.Empty);
            listSelected = onSelected;
            listCancelled = onCancel;
            listMenu.gameObject.SetActive(true);
            listMenu.SetItems(items);
        }

        public void ShowTurnHud(string hint) => turnHud.Show(hint);

        public void UpdateTurnHud(float remainingRatio) => turnHud.SetRemaining(remainingRatio);

        public void ShowPopup(string text) => popup.Show(text);

        public void HideAll()
        {
            HideMenus();
            dialogue.Hide();
            turnHud.Hide();
        }

        /// <summary>테스트용: 확인키 대신 메뉴 고르기</summary>
        public void ChooseMain(int index) => OnMainSelected(index);

        public void ChooseList(int index) => OnListSelected(index);

        void HideMenus()
        {
            mainMenu.gameObject.SetActive(false);
            listMenu.gameObject.SetActive(false);
        }

        // 콜백은 한 번만 — 먼저 지우고 메뉴를 닫은 뒤 호출 (콜백 안에서 새 메뉴를 열 수 있게)
        void OnMainSelected(int index)
        {
            var callback = mainSelected;
            mainSelected = null;
            mainMenu.gameObject.SetActive(false);
            callback?.Invoke(index);
        }

        void OnListSelected(int index)
        {
            var callback = listSelected;
            listSelected = null;
            listCancelled = null;
            listMenu.gameObject.SetActive(false);
            callback?.Invoke(index);
        }

        void OnListCancelled()
        {
            var callback = listCancelled;
            listSelected = null;
            listCancelled = null;
            listMenu.gameObject.SetActive(false);
            callback?.Invoke();
        }
    }
}
