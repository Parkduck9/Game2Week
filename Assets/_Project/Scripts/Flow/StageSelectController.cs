using System.Collections.Generic;
using Game2Week.Core;
using Game2Week.Save;
using Game2Week.UI;
using UnityEngine;

namespace Game2Week.Flow
{
    /// <summary>스테이지 선택: 해금된 스테이지만, 최고기록(시간 · 결과) 표시. 취소 → 메인.</summary>
    public sealed class StageSelectController : MonoBehaviour
    {
        [SerializeField] GameSession session;
        [SerializeField] MenuNavigator menu;

        readonly List<string> items = new();

        public IReadOnlyList<string> Items => items;

        void OnEnable()
        {
            menu.Selected += Choose;
            menu.Cancelled += Back;
        }

        void OnDisable()
        {
            menu.Selected -= Choose;
            menu.Cancelled -= Back;
        }

        void Start()
        {
            var ids = session.StageIds;
            int unlocked = session.Save.UnlockedCount(ids);
            items.Clear();
            for (int i = 0; i < unlocked; i++)
            {
                var record = session.Save.Find(ids[i]);
                var recordText = record is { cleared: true }
                    ? $"최고 {SaveService.FormatTime(record.bestTimeSeconds)} · {UiTexts.Outcome(record.bestOutcome)}"
                    : UiTexts.NoRecord;
                items.Add($"{i + 1}. {session.StageName(ids[i])}<pos=58%><color=#A0A4B0>{recordText}</color>");
            }
            menu.SetItems(items);
            // 마지막으로 열린(아직 안 깬) 스테이지에 커서
            menu.Select(Mathf.Max(0, unlocked - 1));
        }

        public void Choose(int index)
        {
            if (session.BeginStage(index)) SceneLoader.Load(SceneNames.Battle);
        }

        public void Back() => SceneLoader.Load(SceneNames.MainMenu);
    }
}
