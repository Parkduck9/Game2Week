using System;
using Game2Week.Stages;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>경기장 위 보석 하나의 표시 (돌기·떠오르기·나타남/사라짐). 동작 규칙은 GemField가 정한다.</summary>
    public sealed class GemView : MonoBehaviour
    {
        [Serializable]
        struct TypeMaterial
        {
            public string type;
            public Material material;
        }

        [SerializeField] Renderer gemRenderer;
        [SerializeField] TypeMaterial[] materials;
        [SerializeField] float spinSpeed = 90f;
        [SerializeField] float bobHeight = 0.06f;
        [SerializeField] float hoverHeight = 0.3f;
        [SerializeField] float popDuration = 0.2f;

        Vector3 baseScale;
        float phase;
        float popTime;
        bool visible;

        public string GemId { get; private set; }
        public string GemType { get; private set; }
        public bool IsVisible => visible;

        void Awake()
        {
            baseScale = transform.localScale;
            phase = UnityEngine.Random.value * 10f;
            gameObject.SetActive(false);
        }

        public void Setup(StageGem gem)
        {
            GemId = gem.id;
            GemType = gem.type;
            name = $"Gem_{gem.id}_{gem.type}";
            foreach (var m in materials)
                if (m.type == gem.type && m.material) gemRenderer.sharedMaterial = m.material;
        }

        public void Show()
        {
            visible = true;
            popTime = 0f;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            visible = false;
            gameObject.SetActive(false);
        }

        void Update()
        {
            phase += Time.deltaTime;
            popTime += Time.deltaTime;
            float pop = Mathf.Clamp01(popTime / popDuration);
            transform.localScale = baseScale * Mathf.SmoothStep(0f, 1f, pop);
            var p = transform.localPosition;
            p.y = hoverHeight + Mathf.Sin(phase * 2.5f) * bobHeight;
            transform.localPosition = p;
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }
    }
}
