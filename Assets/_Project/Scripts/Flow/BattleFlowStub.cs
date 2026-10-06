using Game2Week.Battle;
using Game2Week.Core;
using Game2Week.UI;
using TMPro;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>
    /// 임시: 실제 전투(BattleController)가 생기기 전까지 씬 흐름을 확인하기 위한 대역.
    /// 결과를 직접 골라 Result 씬으로 넘어간다. 06단계에서 BattleController로 교체하고 삭제한다.
    /// </summary>
    public sealed class BattleFlowStub : MonoBehaviour
    {
        static readonly (string label, BattleOutcome outcome)[] Choices =
        {
            ("적을 쓰러뜨린 것으로 끝내기", BattleOutcome.EnemyDefeated),
            ("살려 준 것으로 끝내기", BattleOutcome.EnemySpared),
            ("패배한 것으로 끝내기", BattleOutcome.PlayerDefeated),
        };

        [SerializeField] GameSession session;
        [SerializeField] MenuNavigator menu;
        [SerializeField] TMP_Text infoText;

        void OnEnable() => menu.Selected += OnSelected;

        void OnDisable() => menu.Selected -= OnSelected;

        void Start()
        {
            var enemy = session.CurrentEnemy;
            infoText.text = $"전투 씬 (준비 중)\n상대: {(enemy ? enemy.DisplayName : "없음")}";
            var labels = new string[Choices.Length];
            for (int i = 0; i < Choices.Length; i++) labels[i] = Choices[i].label;
            menu.SetItems(labels);
        }

        void OnSelected(int index) => Finish(Choices[index].outcome);

        public void Finish(BattleOutcome outcome)
        {
            session.EndBattle(outcome);
            SceneLoader.Load(SceneNames.Result);
        }
    }
}
