using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>
    /// 탄막 턴 주인공 이동: 바닥 평면 8방향 (점프·대시 없음). 카메라가 +Z를 보므로 ↑ = +Z.
    /// 리깅 전이라 걷는 느낌은 모델을 통통 튀게 해서 낸다.
    /// </summary>
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float speed = 2.8f;
        [SerializeField, Min(0.05f)] float radius = 0.22f;
        [SerializeField] float turnSpeed = 720f;
        [SerializeField] float bounceHeight = 0.06f;
        [SerializeField] float bounceSpeed = 14f;
        [Tooltip("통통 튈 모델 (없으면 첫 번째 자식)")]
        [SerializeField] Transform model;

        BattleArena arena;
        float bouncePhase;
        Vector3 modelBase;

        public float Radius => radius;

        Renderer[] renderers;

        /// <summary>무적 시간 깜빡임 (보였다 안 보였다)</summary>
        public void SetVisible(bool visible)
        {
            renderers ??= GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.enabled = visible;
        }

        public void Init(BattleArena battleArena)
        {
            arena = battleArena;
            if (!model && transform.childCount > 0) model = transform.GetChild(0);
            if (model) modelBase = model.localPosition;
        }

        public void Teleport(Vector3 position)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);
            bouncePhase = 0f;
            if (model) model.localPosition = modelBase;
        }

        public void Move(Vector2 input, float deltaTime)
        {
            var dir = new Vector3(input.x, 0f, input.y);
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            bool moving = dir.sqrMagnitude > 0.0001f;

            if (moving)
            {
                var next = transform.position + dir * (speed * deltaTime);
                transform.position = arena ? arena.ClampToArena(next, radius) : next;
                var look = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * deltaTime);
                bouncePhase += deltaTime * bounceSpeed;
            }
            else bouncePhase = 0f;

            if (model) model.localPosition = modelBase + Vector3.up * (Mathf.Abs(Mathf.Sin(bouncePhase)) * bounceHeight);
        }
    }
}
