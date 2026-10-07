using Game2Week.Data;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>이동 모델의 결과를 경기장 위치와 캐릭터 외형에 적용한다.</summary>
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] float radius = 0.22f;
        [SerializeField] float turnSpeed = 720f;
        [SerializeField] Transform model;
        /// <summary>GroundVelocity가 실제 속도를 따라가는 빠르기 (1/초)</summary>
        const float VelocitySmoothing = 8f;

        BattleArena arena;
        PlayerActionSettings settings;
        PlayerActionView actionView;
        BattleFeedback feedback;
        float groundY;
        bool ownsSettings;
        Vector3 modelBase;

        public float Radius => radius;
        public float BodyHeight => settings.bodyHeight;
        public float ParryReach => settings.parryReach;
        public PlayerMotorModel Motor { get; private set; }
        public Vector3 PreviousPosition { get; private set; }
        public int ParrySuccesses { get; private set; }
        public bool CanContactEnemy => Motor.IsGrounded && !Motor.Dodging;
        /// <summary>마지막 이동에서 실제로 움직인 수평 속도 (m/s) — 벽에 막히면 입력이 있어도 0에 가깝다</summary>
        public float GroundSpeed { get; private set; }
        /// <summary>최근 수평 이동 속도 벡터 (부드럽게, 이동 속도 이하로 제한) — 탄 예측 조준용. 회피 순간 속도로 튀지 않는다.</summary>
        public Vector3 GroundVelocity { get; private set; }
        public PlayerActionSettings Settings => settings;

        Renderer[] renderers;

        /// <summary>무적 시간 깜빡임 (보였다 안 보였다)</summary>
        public void SetVisible(bool visible)
        {
            renderers ??= GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) if (r) r.enabled = visible;
        }

        public void Init(BattleArena battleArena, PlayerActionSettings actionSettings = null, BattleFeedback battleFeedback = null)
        {
            arena = battleArena;
            feedback = battleFeedback;
            settings = actionSettings;
            if (!settings) { settings = ScriptableObject.CreateInstance<PlayerActionSettings>(); ownsSettings = true; }
            Motor = new PlayerMotorModel(settings);
            if (!model && transform.childCount > 0) model = transform.GetChild(0);
            if (model) modelBase = model.localPosition;
            if (!TryGetComponent(out actionView)) actionView = gameObject.AddComponent<PlayerActionView>();
            actionView.Init(this, model);
        }

        public void Teleport(Vector3 position)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);
            groundY = position.y;
            PreviousPosition = position;
            Motor.Reset();
            ParrySuccesses = 0;
            GroundSpeed = 0f;
            GroundVelocity = Vector3.zero;
            if (model) model.localPosition = modelBase;
        }

        public void Move(Vector2 worldInput, float dt, Vector3? facing = null)
        {
            PreviousPosition = transform.position;
            bool wasDodging = Motor.Dodging, wasAirborne = Motor.Airborne, wasBraced = Motor.BraceReady;
            Motor.Tick(worldInput, dt);
            var delta = Motor.Displacement;
            var next = transform.position + new Vector3(delta.x, 0f, delta.y);
            next.y = groundY + Motor.Height;
            transform.position = arena ? arena.ClampToArena(next, radius) : next;
            if (feedback != null)
            {
                var feet = transform.position; feet.y = groundY;
                if (!wasDodging && Motor.Dodging) feedback.RaisePlayerDodged(feet);
                if (!wasAirborne && Motor.Airborne) feedback.RaisePlayerJumped(feet);
                if (wasAirborne && !Motor.Airborne) feedback.RaisePlayerLanded(feet);
                if (!wasBraced && Motor.BraceReady) feedback.RaisePlayerBraced(feet);
            }
            var moved = transform.position - PreviousPosition; moved.y = 0f;
            GroundSpeed = dt > 0f ? moved.magnitude / dt : 0f;
            if (dt > 0f)
            {
                var raw = Vector3.ClampMagnitude(moved / dt, settings.moveSpeed);
                GroundVelocity = Vector3.Lerp(GroundVelocity, raw, 1f - Mathf.Exp(-VelocitySmoothing * dt));
            }
            if (Motor.Bracing && facing == null) return; // 자세 중에는 방향 유지
            var dir = facing ?? new Vector3(worldInput.x, 0f, worldInput.y);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * dt);
        }
        public void OnParrySuccess(Vector3 incoming)
        {
            ParrySuccesses++;
            float side = Vector3.Dot(incoming - transform.position, transform.right) > 0f ? -1f : 1f;
            actionView.PlayDeflection(side);
            feedback?.RaisePlayerParried(incoming, side);
        }
        public void StopActions()
        {
            Motor.Reset();
            var position = transform.position; position.y = groundY; transform.position = position;
            PreviousPosition = position;
            GroundSpeed = 0f;
            GroundVelocity = Vector3.zero;
        }
        void OnDestroy() { if (ownsSettings && settings) Destroy(settings); }
    }
}
