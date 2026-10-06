using Game2Week.Battle;
using Game2Week.Core;
using Game2Week.UI;
using TMPro;
using UnityEngine;

namespace Game2Week.Flow
{
    public sealed class ResultController : MonoBehaviour
    {
        public const int RetryIndex = 0;
        public const int MainMenuIndex = 1;
        public const int QuitIndex = 2;
        static readonly string[] Items = { "다시 도전", "메인 화면으로", "게임 종료" };

        [SerializeField] GameSession session;
        [SerializeField] MenuNavigator menu;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text messageText;
        [SerializeField] Color victoryColor = new(1f, 0.85f, 0.3f);
        [SerializeField] Color defeatColor = new(0.9f, 0.25f, 0.25f);

        public string Title => titleText.text;

        void OnEnable() => menu.Selected += Choose;

        void OnDisable() => menu.Selected -= Choose;

        void Start()
        {
            ShowOutcome(session.LastOutcome);
            menu.SetItems(Items);
        }

        void ShowOutcome(BattleOutcome? outcome)
        {
            var enemy = session.CurrentEnemy;
            (string title, string message, Color color) = outcome switch
            {
                BattleOutcome.EnemyDefeated => ("승리", enemy ? enemy.DefeatText : string.Empty, victoryColor),
                BattleOutcome.EnemySpared => ("전투 종료", enemy ? enemy.SpareText : string.Empty, victoryColor),
                BattleOutcome.PlayerDefeated => ("GAME OVER", "* 포기하지 마...", defeatColor),
                _ => ("결과 없음", "* 전투를 거치지 않고 이 화면에 왔다.", Color.white),
            };
            titleText.text = title;
            titleText.color = color;
            messageText.text = message;
        }

        public void Choose(int index)
        {
            switch (index)
            {
                case RetryIndex:
                    session.BeginBattle(session.CurrentEnemy);
                    SceneLoader.Load(SceneNames.Battle);
                    break;
                case MainMenuIndex:
                    SceneLoader.Load(SceneNames.MainMenu);
                    break;
                case QuitIndex:
                    GameQuit.Quit();
                    break;
            }
        }
    }
}
