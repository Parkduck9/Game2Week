using Game2Week.Core;
using Game2Week.UI;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>
    /// 전투 중 ESC 일시정지: 시간 정지 + 전투 UI 숨김 + 계속 / 설정 / 메인으로.
    /// 다시 ESC(또는 취소키)로 계속. 풀면 입력 모드(탄막 턴 이동 / 메뉴)를 원래대로.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        public const int ResumeIndex = 0;
        public const int SettingsIndex = 1;
        public const int QuitIndex = 2;

        [SerializeField] InputReader input;
        [SerializeField] GameSession session;
        [SerializeField] GameObject panel;
        [SerializeField] MenuNavigator menu;
        [SerializeField] SettingsPanel settings;
        [SerializeField] ConfirmPopup confirm;
        [Tooltip("일시정지 동안 숨길 전투 UI (입력이 겹치지 않게 꺼 둔다)")]
        [SerializeField] GameObject battleUiRoot;

        InputReader.Mode savedMode;

        public bool IsPaused { get; private set; }
        /// <summary>전투가 끝나면 BattleController가 false로</summary>
        public bool CanPause { get; set; } = true;
        public MenuNavigator Menu => menu;

        void Awake()
        {
            menu.Selected += OnSelected;
            menu.Cancelled += Resume;
            panel.SetActive(false);
        }

        void OnEnable() => input.Pause += OnPausePressed;

        void OnDisable() => input.Pause -= OnPausePressed;

        void OnDestroy()
        {
            menu.Selected -= OnSelected;
            menu.Cancelled -= Resume;
            if (IsPaused) Time.timeScale = 1f;
        }

        void OnPausePressed()
        {
            if (!IsPaused) Pause();
            else if (!settings.IsOpen && !confirm.IsOpen) Resume();
        }

        public void Pause()
        {
            if (IsPaused || !CanPause || SceneLoader.IsLoading) return;
            IsPaused = true;
            savedMode = input.CurrentMode;
            Time.timeScale = 0f;
            battleUiRoot.SetActive(false);
            panel.SetActive(true);
            ShowMenu(ResumeIndex);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            menu.gameObject.SetActive(false);
            panel.SetActive(false);
            battleUiRoot.SetActive(true);
            Time.timeScale = 1f;
            input.Restore(savedMode);
        }

        void ShowMenu(int cursor)
        {
            menu.gameObject.SetActive(true);
            menu.SetItems(UiTexts.PauseMenu);
            menu.Select(cursor);
        }

        void OnSelected(int index)
        {
            switch (index)
            {
                case ResumeIndex:
                    Resume();
                    break;
                case SettingsIndex:
                    menu.gameObject.SetActive(false);
                    settings.Open(session.Save, () => ShowMenu(SettingsIndex));
                    break;
                case QuitIndex:
                    menu.gameObject.SetActive(false);
                    confirm.Show(UiTexts.ConfirmQuitBattle, QuitToMain, () => ShowMenu(QuitIndex));
                    break;
            }
        }

        void QuitToMain()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            SceneLoader.Load(SceneNames.MainMenu);
        }
    }
}
