using TMPro;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>적 HP 막대 — 적이 맞았을 때 잠깐 나타난다 (BattleEvents 구독).</summary>
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text nameText;
        [SerializeField] BarView bar;
        [SerializeField, Min(0.2f)] float showSeconds = 2f;

        BattleEvents events;
        float timer;

        public bool IsVisible => panel.activeSelf;
        public float Ratio => bar.Ratio;

        void Awake() => panel.SetActive(false);

        public void Bind(BattleEvents battleEvents, string enemyName, int hp, int maxHp)
        {
            Unbind();
            events = battleEvents;
            events.EnemyHpChanged += OnHp;
            nameText.text = enemyName;
            bar.SetRatio(maxHp > 0 ? (float)hp / maxHp : 0f);
        }

        void OnDestroy() => Unbind();

        void Unbind()
        {
            if (events != null) events.EnemyHpChanged -= OnHp;
            events = null;
        }

        void OnHp(int hp, int maxHp)
        {
            bar.SetRatio(maxHp > 0 ? (float)hp / maxHp : 0f);
            panel.SetActive(true);
            timer = showSeconds;
        }

        void Update()
        {
            if (!panel.activeSelf) return;
            timer -= Time.deltaTime;
            if (timer <= 0f) panel.SetActive(false);
        }
    }
}
