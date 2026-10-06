using TMPro;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>탄막 턴 표시: 안내 문구 + 남은 시간 막대.</summary>
    public sealed class TurnHud : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text hintText;
        [SerializeField] BarView timeBar;

        public void Show(string hint)
        {
            panel.SetActive(true);
            hintText.text = hint;
            timeBar.SetRatio(1f);
        }

        public void SetRemaining(float ratio) => timeBar.SetRatio(ratio);

        public void Hide() => panel.SetActive(false);
    }
}
