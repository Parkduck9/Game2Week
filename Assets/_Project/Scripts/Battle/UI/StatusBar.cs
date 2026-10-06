using TMPro;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>상태 줄: 이름 · LV · HP 막대 · HP 숫자. BattleEvents를 구독해서만 갱신된다.</summary>
    public sealed class StatusBar : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text hpText;
        [SerializeField] BarView hpBar;

        BattleEvents events;

        public string HpText => hpText.text;

        public void Bind(BattleEvents battleEvents, string displayName, int level, int hp, int maxHp)
        {
            Unbind();
            events = battleEvents;
            events.PlayerHpChanged += OnHp;
            nameText.text = $"{displayName}   LV {level}";
            OnHp(hp, maxHp);
        }

        void OnDestroy() => Unbind();

        void Unbind()
        {
            if (events != null) events.PlayerHpChanged -= OnHp;
            events = null;
        }

        void OnHp(int hp, int maxHp)
        {
            hpText.text = $"{hp} / {maxHp}";
            hpBar.SetRatio(maxHp > 0 ? (float)hp / maxHp : 0f);
        }
    }
}
