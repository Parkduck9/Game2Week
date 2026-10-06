using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game2Week.UI
{
    /// <summary>
    /// 메뉴 항목 표시. 템플릿 텍스트를 복제해 항목을 만들고, 선택된 항목을 강조한다 (비활성 항목은 회색).
    /// 배치는 부모의 LayoutGroup이 맡는다.
    /// </summary>
    public sealed class MenuListView : MonoBehaviour
    {
        [SerializeField] TMP_Text itemTemplate;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color selectedColor = new(1f, 0.85f, 0.3f);
        [SerializeField] Color disabledColor = new(0.45f, 0.45f, 0.5f);
        [SerializeField] string selectedPrefix = "> ";
        [SerializeField] string normalPrefix = "   ";

        readonly List<TMP_Text> texts = new();
        readonly List<string> labels = new();
        readonly HashSet<int> disabled = new();

        public IReadOnlyList<string> Labels => labels;

        public void SetItems(IReadOnlyList<string> items, IEnumerable<int> disabledIndices = null)
        {
            foreach (var t in texts) Destroy(t.gameObject);
            texts.Clear();
            labels.Clear();
            disabled.Clear();
            if (disabledIndices != null) disabled.UnionWith(disabledIndices);
            itemTemplate.gameObject.SetActive(false);

            foreach (var label in items)
            {
                var text = Instantiate(itemTemplate, itemTemplate.transform.parent);
                text.name = $"Item_{texts.Count}";
                text.gameObject.SetActive(true);
                texts.Add(text);
                labels.Add(label);
            }
        }

        /// <summary>라벨만 바꾼다 (설정 화면처럼 값이 바뀔 때).</summary>
        public void SetLabel(int index, string label)
        {
            if (index >= 0 && index < labels.Count) labels[index] = label;
        }

        public void SetSelected(int index)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                bool selected = i == index;
                texts[i].text = (selected ? selectedPrefix : normalPrefix) + labels[i];
                texts[i].color = disabled.Contains(i) ? disabledColor : selected ? selectedColor : normalColor;
            }
        }
    }
}
