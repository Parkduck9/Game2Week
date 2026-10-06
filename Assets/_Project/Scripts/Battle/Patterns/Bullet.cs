using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 탄 기본: 바닥 평면 위를 직선으로 날아가고, 주인공에게 닿으면 알린다 (물리 엔진 없이 거리 판정).
    /// 다른 움직임이 필요하면 상속해서 Move를 바꾼다.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] float radius = 0.14f;
        [SerializeField] float spinSpeed = 360f;
        [SerializeField] bool parryable;

        Vector3 velocity;

        public float Radius => radius;
        public bool Active { get; private set; }
        public bool Deflected { get; private set; }
        Vector3 deflectStart, deflectShoulder, deflectEnd;
        float deflectTime;

        public void Launch(Vector3 position, Vector3 flatVelocity)
        {
            transform.position = position;
            velocity = flatVelocity;
            if (velocity.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(velocity);
            Active = true;
            Deflected = false;
            deflectTime = 0f;
            OnLaunch(position, flatVelocity);
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            Active = false;
            gameObject.SetActive(false);
        }

        /// <summary>움직이고, 주인공과 닿았으면 true.</summary>
        public bool Tick(float deltaTime, PatternContext context)
        {
            if (!Active || deltaTime <= 0f) return false;
            int count = Mathf.Max(1, Mathf.CeilToInt(deltaTime / .015f));
            for (int i = 0; i < count && Active; i++)
            {
                var playerFrom = Vector3.Lerp(context.PreviousPlayerPosition,context.Player.position,i/(float)count);
                var playerTo = Vector3.Lerp(context.PreviousPlayerPosition,context.Player.position,(i+1f)/count);
                if (TickSegment(deltaTime/count,context,playerFrom,playerTo)) return true;
            }
            return false;
        }
        bool TickSegment(float deltaTime, PatternContext context, Vector3 playerFrom, Vector3 playerTo)
        {
            var before = transform.position;
            if (Deflected)
            {
                deflectTime += deltaTime;
                float u = Mathf.Clamp01(deflectTime / 0.24f);
                transform.position = (1f - u) * (1f - u) * deflectStart + 2f * (1f - u) * u * deflectShoulder + u * u * deflectEnd;
                if (u >= 1f) Deactivate();
                return false;
            }
            Move(deltaTime);
            transform.Rotate(0f, 0f, spinSpeed * deltaTime, Space.Self);
            var mover = context.PlayerMover;
            if (parryable && mover && mover.Motor.CanParry &&
                Vector3.Dot(before - context.Player.position, context.Player.forward) >= -0.15f &&
                AttackGeometry.SweptBody(before, transform.position, playerFrom, playerTo,
                    mover.ParryReach + radius, -radius, context.PlayerBodyHeight + radius))
            {
                Deflected = true;
                deflectStart = transform.position;
                var direction = AttackGeometry.DeflectionDirection(before, context.Player.position, context.Player.forward, context.Player.right);
                float side = Vector3.Dot(direction, context.Player.right) > 0f ? 1f : -1f;
                deflectShoulder = context.Player.position + context.Player.right * side * 0.7f + Vector3.up * 0.65f;
                deflectEnd = context.Player.position + direction * 2.2f + Vector3.up * 0.55f;
                mover.OnParrySuccess(before);
                return false;
            }
            bool hit = AttackGeometry.SweptBody(before, transform.position, playerFrom, playerTo,
                radius + context.PlayerRadius, -radius, context.PlayerBodyHeight + radius);
            if (hit) return true;
            if (context.IsOutside(transform.position, 0.5f))
            {
                Deactivate();
                return false;
            }
            return false;
        }

        protected virtual void Move(float deltaTime) => transform.position += velocity * deltaTime;
        protected virtual void OnLaunch(Vector3 position, Vector3 launchVelocity) { }
    }
}
