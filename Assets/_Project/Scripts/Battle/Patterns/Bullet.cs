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

        Vector3 velocity;

        public float Radius => radius;
        public bool Active { get; private set; }

        public void Launch(Vector3 position, Vector3 flatVelocity)
        {
            transform.position = position;
            velocity = new Vector3(flatVelocity.x, 0f, flatVelocity.z);
            if (velocity.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(velocity);
            Active = true;
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
            if (!Active) return false;
            Move(deltaTime);
            transform.Rotate(0f, 0f, spinSpeed * deltaTime, Space.Self);
            if (context.IsOutside(transform.position, 0.5f))
            {
                Deactivate();
                return false;
            }
            return PatternContext.FlatDistance(transform.position, context.Player.position) <= radius + context.PlayerRadius;
        }

        protected virtual void Move(float deltaTime) => transform.position += velocity * deltaTime;
    }
}
