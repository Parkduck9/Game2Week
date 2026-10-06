using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 적의 3D 표시와 연출. 지금은 임시 도형이라 Animator 대신 코드로 움직이고,
    /// 나중에 모델이 생기면 같은 메서드(PlayHit 등)에서 Animator를 부르도록 바꾼다.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour
    {
        enum State
        {
            Idle,
            Defeated,
            Spared,
        }

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] Transform body;
        [SerializeField] Renderer[] flashRenderers;
        [SerializeField] float idleBobHeight = 0.04f;
        [SerializeField] float idleBobSpeed = 2.2f;
        [SerializeField] float hitShake = 0.08f;
        [SerializeField] float endDuration = 1f;
        [Tooltip("적에게 닿았다고 판정하는 반지름 (m)")]
        [SerializeField, Min(0.1f)] float contactRadius = 0.55f;

        State state;
        Vector3 baseLocalPos;
        Vector3 baseScale;
        Color[] baseColors;
        MaterialPropertyBlock block;
        float time;
        float flash;
        float shake;
        float attack;
        float endTime;

        public float ContactRadius => contactRadius;

        void Awake()
        {
            if (!body) body = transform;
            baseLocalPos = body.localPosition;
            baseScale = body.localScale;
            block = new MaterialPropertyBlock();
            baseColors = new Color[flashRenderers.Length];
            for (int i = 0; i < flashRenderers.Length; i++)
                baseColors[i] = flashRenderers[i].sharedMaterial.HasProperty(BaseColorId) ? flashRenderers[i].sharedMaterial.GetColor(BaseColorId) : Color.white;
        }

        public void PlayHit()
        {
            flash = 1f;
            shake = 1f;
        }

        public void PlayAttack() => attack = 1f;

        public void PlayDefeated() => End(State.Defeated);

        public void PlaySpared() => End(State.Spared);

        void End(State next)
        {
            state = next;
            endTime = 0f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            time += dt;
            flash = Mathf.MoveTowards(flash, 0f, dt * 4f);
            shake = Mathf.MoveTowards(shake, 0f, dt * 3f);
            attack = Mathf.MoveTowards(attack, 0f, dt * 2f);

            switch (state)
            {
                case State.Idle:
                    float bob = Mathf.Sin(time * idleBobSpeed) * idleBobHeight;
                    float hop = Mathf.Sin(attack * Mathf.PI) * 0.25f;
                    var offset = shake > 0f ? Random.insideUnitSphere * hitShake * shake : Vector3.zero;
                    body.localPosition = baseLocalPos + new Vector3(offset.x, bob + hop, offset.z);
                    float squash = 1f + Mathf.Sin(time * idleBobSpeed) * 0.03f;
                    body.localScale = Vector3.Scale(baseScale, new Vector3(1f / squash, squash, 1f / squash));
                    break;

                case State.Defeated:
                    endTime += dt;
                    float k = Mathf.Clamp01(endTime / endDuration);
                    body.localScale = baseScale * (1f - k);
                    body.localPosition = baseLocalPos + Vector3.down * k * 0.3f;
                    flash = Mathf.Max(flash, 1f - k);
                    break;

                case State.Spared:
                    endTime += dt;
                    float s = Mathf.Clamp01(endTime / endDuration);
                    body.localPosition = baseLocalPos + Vector3.up * s * 0.8f;
                    body.Rotate(0f, 180f * dt, 0f, Space.Self);
                    body.localScale = baseScale * Mathf.Lerp(1f, 0.6f, s);
                    break;
            }

            for (int i = 0; i < flashRenderers.Length; i++)
            {
                flashRenderers[i].GetPropertyBlock(block);
                block.SetColor(BaseColorId, Color.Lerp(baseColors[i], Color.white, flash));
                flashRenderers[i].SetPropertyBlock(block);
            }
        }
    }
}
