using System.Collections.Generic;
using Game2Week.Battle.Patterns;
using Game2Week.Battle.View;
using TMPro;
using UnityEngine;

namespace Game2Week.Battle.UI
{
    /// <summary>탄막 중 빨강·파랑이 처음 보이면 화면 위쪽에 대응 방법을 잠깐 띄운다 (전투마다 색당 한 번).</summary>
    public sealed class ColorGuideView : MonoBehaviour
    {
        [SerializeField] BattleWorld world;
        [SerializeField] TMP_Text label;
        [SerializeField, Min(0.5f)] float showSeconds = 3f;

        readonly ColorGuideModel model = new();
        readonly List<ThreatPoint> threats = new();
        readonly List<AttackColor> colors = new();
        float shownLeft;

        public ColorGuideModel Model => model;
        // 이 컴포넌트가 문구와 같은 오브젝트에 있을 수 있으므로 오브젝트가 아니라 글자 표시만 켜고 끈다
        public bool IsShowing => label && label.enabled;
        public string Text => label ? label.text : null;

        void Awake() { if (label) label.enabled = false; }

        void LateUpdate()
        {
            if (!label || !world || !world.Patterns || Time.timeScale <= 0f) return;
            if (shownLeft > 0f)
            {
                shownLeft -= Time.deltaTime;
                if (shownLeft <= 0f) label.enabled = false;
            }
            if (world.Patterns.Current is not IThreatSource source) return;

            threats.Clear();
            colors.Clear();
            source.CollectThreats(threats);
            foreach (var t in threats) colors.Add(t.Color);
            var first = model.Observe(colors);
            if (first == null) return;

            label.text = first == AttackColor.Red ? BattleTexts.RedGuide : BattleTexts.BlueGuide;
            label.color = BattleTexts.AttackColorTint(first.Value);
            label.enabled = true;
            shownLeft = showSeconds;
        }
    }
}
