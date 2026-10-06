using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game2Week.UI
{
    /// <summary>
    /// 메뉴 항목 표시. 템플릿 텍스트를 복제해 항목을 만들고, 선택된 항목을 강조한다.
    /// 배치는 부모의 LayoutGroup이 맡는다.
    /// </summary>
    public sealed class MenuListView : MonoBehaviour
    {
        [SerializeField] TMP_Text itemTemplate;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color selectedColor = new(1f, 0.85f, 0.3f);
        [SerializeField] string selectedPrefix = "> ";
        [SerializeField] string normalPrefix = "   ";

        readonly List<TMP_Text> texts = new();
        readonly List<string> labels = new();

        public void SetItems(IReadOnlyList<string> items)
        {
            foreach (var t in texts) Destroy(t.gameObject);
            texts.Clear();
            labels.Clear();
            itemTemplate.gameObject.SetActive(false);

            foreach (var label in items)
            {
                var text = Instantiate(itemTemplate, itemTemplate.transform.parent);
                text.name = $"Item_{label}";
                text.gameObject.SetActive(true);
                texts.Add(text);
                labels.Add(label);
            }
        }

        public void SetSelected(int index)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                bool selected = i == index;
                texts[i].text = (selected ? selectedPrefix : normalPrefix) + labels[i];
                texts[i].color = selected ? selectedColor : normalColor;
            }
        }
    }
}
