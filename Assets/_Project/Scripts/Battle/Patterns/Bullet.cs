using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 탄 기본: 바닥 평면 위를 직선으로 날아가고, 주인공에게 닿으면 알린다 (물리 엔진 없이 거리 판정).
    /// 다른 움직임은 Launch에 ITrajectory를 넘기거나, 상속해서 Move를 바꾼다.
    /// 쳐내기는 노랑만, 겹쳤을 때 피해 여부는 PatternContext.ShouldHit(색 규칙)이 정한다.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] float radius = 0.14f;
        [SerializeField] float spinSpeed = 360f;
        [SerializeField] bool parryable;
        [SerializeField] AttackColor color = AttackColor.Yellow;

        // 색만으로 구분하지 않도록 회전도 다르게: 빨강 = 멈춰 있음(정지 자세), 파랑 = 빠르게 돎(움직이기)
        static readonly UnityEngine.Color RedTint = new(1f, 0.25f, 0.25f, 1f);
        static readonly UnityEngine.Color BlueTint = new(0.3f, 0.62f, 1f, 1f);
        const float BlueSpinMultiplier = 2f;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        Vector3 velocity;
        ITrajectory trajectory;
        Renderer[] renderers;
        MaterialPropertyBlock tintBlock;
        TrajectoryLaunch launch;
        float elapsed;

        public float Radius => radius;
        public bool Active { get; private set; }
        public bool Deflected { get; private set; }
        /// <summary>대응 규칙 색. 프리팹 기본값, 패턴이 발사 전에 바꿀 수 있다.</summary>
        public AttackColor Color { get => color; set => color = value; }
        public bool Parryable => parryable && color == AttackColor.Yellow;
        Vector3 deflectStart, deflectShoulder, deflectEnd;
        float deflectTime;

        /// <param name="trajectory">null이면 flatVelocity로 직진 (또는 하위 클래스의 Move)</param>
        public void Launch(Vector3 position, Vector3 flatVelocity, ITrajectory trajectory = null)
        {
            transform.position = position;
            velocity = flatVelocity;
            this.trajectory = trajectory;
            launch = new TrajectoryLaunch(position, flatVelocity);
            elapsed = 0f;
            ApplyTint();
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
            transform.Rotate(0f, 0f, SpinFor(color) * deltaTime, Space.Self);
            var mover = context.PlayerMover;
            if (Parryable && mover && mover.Motor.CanParry &&
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
            if (hit && context.ShouldHit(color)) return true;
            if (context.IsOutside(transform.position, 0.5f))
            {
                Deactivate();
                return false;
            }
            return false;
        }

        float SpinFor(AttackColor c) => c switch
        {
            AttackColor.Red => 0f,
            AttackColor.Blue => spinSpeed * BlueSpinMultiplier,
            _ => spinSpeed,
        };

        /// <summary>노랑은 프리팹 재질 그대로, 빨강·파랑은 색을 덮어쓴다 (풀에서 재사용돼도 매 발사마다 갱신).</summary>
        void ApplyTint()
        {
            renderers ??= GetComponentsInChildren<Renderer>(true);
            if (color == AttackColor.Yellow)
            {
                foreach (var r in renderers) if (r) r.SetPropertyBlock(null);
                return;
            }
            tintBlock ??= new MaterialPropertyBlock();
            var tint = color == AttackColor.Red ? RedTint : BlueTint;
            tintBlock.SetColor(BaseColorId, tint);
            tintBlock.SetColor(ColorId, tint);
            foreach (var r in renderers) if (r) r.SetPropertyBlock(tintBlock);
        }

        protected virtual void Move(float deltaTime)
        {
            if (trajectory == null)
            {
                transform.position += velocity * deltaTime;
                return;
            }
            elapsed += deltaTime;
            transform.position = trajectory.Evaluate(launch, elapsed);
        }
        protected virtual void OnLaunch(Vector3 position, Vector3 launchVelocity) { }
    }
}
