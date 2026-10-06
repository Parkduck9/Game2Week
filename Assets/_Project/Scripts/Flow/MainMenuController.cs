using Game2Week.Core;
using Game2Week.UI;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>메인: 새로 시작(진행 있으면 확인) · 이어하기(진행 없으면 비활성) · 설정 · 종료</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        public const int NewGameIndex = 0;
        public const int ContinueIndex = 1;
        public const int SettingsIndex = 2;
        public const int QuitIndex = 3;

        [SerializeField] GameSession session;
        [SerializeField] MenuNavigator menu;
        [SerializeField] ConfirmPopup confirm;
        [SerializeField] SettingsPanel settings;

        public MenuNavigator Menu => menu;
        public ConfirmPopup Confirm => confirm;
        public SettingsPanel Settings => settings;

        void OnEnable() => menu.Selected += Choose;

        void OnDisable() => menu.Selected -= Choose;

        void Start()
        {
            session.ResetSession();
            session.ApplySettingsOnce();
            ShowMenu(NewGameIndex);
        }

        public bool CanContinue => session.Save.HasProgress;

        void ShowMenu(int cursor)
        {
            menu.gameObject.SetActive(true);
            menu.SetItems(UiTexts.MainMenu, CanContinue ? null : new[] { ContinueIndex });
            menu.Select(cursor);
        }

        public void Choose(int index)
        {
            switch (index)
            {
                case NewGameIndex:
                    if (!CanContinue)
                    {
                        StartNewGame();
                        break;
                    }
                    menu.gameObject.SetActive(false);
                    confirm.Show(UiTexts.ConfirmNewGame, StartNewGame, () => ShowMenu(NewGameIndex));
                    break;
                case ContinueIndex:
                    if (CanContinue) SceneLoader.Load(SceneNames.StageSelect);
                    break;
                case SettingsIndex:
                    menu.gameObject.SetActive(false);
                    settings.Open(session.Save, () => ShowMenu(SettingsIndex));
                    break;
                case QuitIndex:
                    GameQuit.Quit();
                    break;
            }
        }

        void StartNewGame()
        {
            session.Save.ResetProgress();
            SceneLoader.Load(SceneNames.StageSelect);
        }
    }
}
