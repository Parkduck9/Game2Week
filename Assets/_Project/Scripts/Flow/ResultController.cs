using System;
using System.Collections.Generic;
using System.Linq;
using Game2Week.Battle;
using Game2Week.Core;
using Game2Week.Save;
using Game2Week.UI;
using TMPro;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>
    /// 결과: 승리면 클리어 시간·신기록, 메뉴는 상황에 따라
    /// (다음 스테이지 | 엔딩으로) · 다시 도전 · 스테이지 선택.
    /// </summary>
    public sealed class ResultController : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] MenuNavigator menu;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text messageText;
        [SerializeField] TMP_Text timeText;
        [SerializeField] Color victoryColor = new(1f, 0.85f, 0.3f);
        [SerializeField] Color defeatColor = new(0.9f, 0.25f, 0.25f);

        readonly List<(string label, Action action)> entries = new();

        public string Title => titleText.text;
        public string TimeText => timeText ? timeText.text : string.Empty;
        public IReadOnlyList<string> Items => entries.Select(e => e.label).ToList();

        void OnEnable() => menu.Selected += Choose;

        void OnDisable() => menu.Selected -= Choose;

        void Start()
        {
            ShowOutcome(session.LastOutcome);
            BuildMenu(session.LastOutcome);
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

            if (!timeText) return;
            bool won = outcome is BattleOutcome.EnemyDefeated or BattleOutcome.EnemySpared;
            timeText.text = won
                ? $"클리어 시간 {SaveService.FormatTime(session.LastClearTime)}" + (session.LastIsNewRecord ? $"   <color=#FFD84A>{UiTexts.NewRecord}</color>" : string.Empty)
                : string.Empty;
        }

        void BuildMenu(BattleOutcome? outcome)
        {
            entries.Clear();
            int index = Mathf.Max(0, session.StageIndex);
            bool won = outcome is BattleOutcome.EnemyDefeated or BattleOutcome.EnemySpared;

            if (won && session.HasNextStage)
                entries.Add((UiTexts.NextStage, () => StartStage(index + 1)));
            else if (won && session.IsLastStage && !session.Save.Progress.endingSeen)
                entries.Add((UiTexts.ToEnding, () => SceneLoader.Load(SceneNames.Ending)));

            if (outcome != null) entries.Add((UiTexts.Retry, () => StartStage(index)));
            entries.Add((UiTexts.StageSelect, () => SceneLoader.Load(SceneNames.StageSelect)));
        }

        void StartStage(int index)
        {
            if (session.BeginStage(index)) SceneLoader.Load(SceneNames.Battle);
        }

        public void Choose(int index)
        {
            if (index >= 0 && index < entries.Count) entries[index].action();
        }

        public void Choose(string label) => Choose(entries.FindIndex(e => e.label == label));
    }
}
