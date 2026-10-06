using Game2Week.Core;
using Game2Week.UI;
using UnityEngine;

namespace Game2Week.Flow
{
    public sealed class MainMenuController : MonoBehaviour
    {
        public const int StartIndex = 0;
        public const int QuitIndex = 1;
        static readonly string[] Items = { "시작", "종료" };

        [SerializeField] GameSession session;
        [SerializeField] MenuNavigator menu;

        void OnEnable() => menu.Selected += Choose;

        void OnDisable() => menu.Selected -= Choose;

        void Start()
        {
            session.ResetSession();
            menu.SetItems(Items);
        }

        public void Choose(int index)
        {
            switch (index)
            {
                case StartIndex:
                    if (session.BeginStage(0)) SceneLoader.Load(SceneNames.Battle);
                    break;
                case QuitIndex:
                    GameQuit.Quit();
                    break;
            }
        }
    }
}
