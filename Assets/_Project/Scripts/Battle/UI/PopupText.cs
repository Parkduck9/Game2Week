using TMPro;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>잠깐 떠올랐다 사라지는 문구 (보석 보상 등).</summary>
    public sealed class PopupText : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] float duration = 1.4f;
        [SerializeField] float rise = 40f;

        RectTransform rect;
        Vector2 basePos;
        float time = float.MaxValue;

        void Awake()
        {
            rect = (RectTransform)text.transform;
            basePos = rect.anchoredPosition;
            text.gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            text.text = message;
            text.gameObject.SetActive(true);
            time = 0f;
        }

        void Update()
        {
            if (time > duration) return;
            time += Time.deltaTime;
            float k = Mathf.Clamp01(time / duration);
            rect.anchoredPosition = basePos + Vector2.up * (rise * k);
            var c = text.color;
            c.a = 1f - k * k;
            text.color = c;
            if (time > duration) text.gameObject.SetActive(false);
        }
    }
}
