using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>가로 막대 (HP, 남은 시간). 채움 이미지의 오른쪽 앵커를 비율로 바꾼다.</summary>
    public sealed class BarView : MonoBehaviour
    {
        [SerializeField] RectTransform fill;

        public float Ratio { get; private set; } = 1f;

        public void SetRatio(float ratio)
        {
            Ratio = Mathf.Clamp01(ratio);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(Ratio, 1f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
        }
    }
}
