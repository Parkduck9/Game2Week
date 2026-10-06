using System;
using System.Collections.Generic;
using Game2Week.Core;
using UnityEngine;

namespace Game2Week.UI
{
    /// <summary>InputReader 입력으로 메뉴 커서를 움직이고, 확인/취소를 알린다.</summary>
    public sealed class MenuNavigator : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] MenuListView view;
        [Tooltip("켜면 좌우, 끄면 상하로 이동")]
        [SerializeField] bool horizontal;

        readonly MenuList list = new();

        public event Action<int> Selected;
        public event Action Cancelled;

        public int Index => list.Index;

        public void SetItems(IReadOnlyList<string> items)
        {
            list.SetCount(items.Count);
            list.Reset();
            view.SetItems(items);
            view.SetSelected(list.Index);
        }

        void OnEnable()
        {
            input.Navigate += OnNavigate;
            input.Submit += OnSubmit;
            input.Cancel += OnCancel;
            input.EnableUI();
        }

        void OnDisable()
        {
            input.Navigate -= OnNavigate;
            input.Submit -= OnSubmit;
            input.Cancel -= OnCancel;
        }

        void OnNavigate(Vector2Int direction)
        {
            // 위쪽 화살표 = 이전 항목
            int delta = horizontal ? direction.x : -direction.y;
            if (list.Move(delta)) view.SetSelected(list.Index);
        }

        void OnSubmit()
        {
            if (list.Count > 0) Selected?.Invoke(list.Index);
        }

        void OnCancel() => Cancelled?.Invoke();
    }
}
