using System;
using System.Linq;
using Game2Week.Core;
using Game2Week.Save;
using UnityEngine;

namespace Game2Week.UI
{
    /// <summary>
    /// 설정 화면 (메인·일시정지 공용). ↑↓ 줄 이동, ←→ 값 변경, Z = 값 변경/돌아가기, X = 닫기.
    /// 화면 설정은 바로 적용하고, 닫을 때 저장한다.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] GameObject panel;
        [SerializeField] MenuListView view;

        readonly MenuList list = new();
        SettingsModel model;
        SaveService save;
        Action onClosed;
        bool subscribed;

        public bool IsOpen => panel.activeSelf;
        public SettingsModel Model => model;

        void Awake() => panel.SetActive(false);

        void OnDisable() => Subscribe(false);

        public void Open(SaveService saveService, Action closed)
        {
            save = saveService;
            onClosed = closed;
            model = new SettingsModel(save.Settings, DisplaySettings.Available16By9());
            panel.SetActive(true);
            view.SetItems(SettingsModel.Rows.Select(model.Label).ToList());
            list.SetCount(SettingsModel.Rows.Count);
            list.Reset();
            view.SetSelected(list.Index);
            input.EnableUI();
            Subscribe(true);
        }

        /// <summary>테스트·코드용</summary>
        public void Change(SettingRow row, int delta)
        {
            int index = Array.IndexOf(SettingsModel.Rows.ToArray(), row);
            if (!model.Change(row, delta, out bool displayChanged)) return;
            view.SetLabel(index, model.Label(row));
            view.SetSelected(list.Index);
            if (displayChanged) DisplaySettings.Apply(model.Data);
        }

        public void Close()
        {
            if (!IsOpen) return;
            Subscribe(false);
            save.SaveSettings();
            panel.SetActive(false);
            var callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }

        void OnNavigate(Vector2Int direction)
        {
            if (direction.y != 0)
            {
                if (list.Move(-direction.y)) view.SetSelected(list.Index);
            }
            else if (direction.x != 0)
            {
                Change(SettingsModel.Rows[list.Index], direction.x);
            }
        }

        void OnSubmit()
        {
            var row = SettingsModel.Rows[list.Index];
            if (row == SettingRow.Back) Close();
            else Change(row, 1);
        }

        void Subscribe(bool on)
        {
            if (on == subscribed || !input) return;
            if (on)
            {
                input.Navigate += OnNavigate;
                input.Submit += OnSubmit;
                input.Cancel += Close;
            }
            else
            {
                input.Navigate -= OnNavigate;
                input.Submit -= OnSubmit;
                input.Cancel -= Close;
            }
            subscribed = on;
        }
    }
}
