using System;
using Game2Week.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Game2Week.Battle.View
{
    public enum BattleShot
    {
        /// <summary>전투 시작 — 경기장을 옆에서 넓게</summary>
        Intro,
        /// <summary>탄막 턴 — 캐릭터 뒤 허리 높이 추적</summary>
        Overview,
        /// <summary>메뉴·대사 — 적을 정면에서</summary>
        EnemyFocus,
        /// <summary>FIGHT 연출 — 적 가까이</summary>
        AttackCloseUp,
    }

    /// <summary>
    /// 전투 카메라. 샷마다 Cinemachine 카메라가 하나씩 있고, 켜진 카메라로 Brain이 블렌드한다.
    /// 샷 위치는 경기장 크기와 적·주인공 위치에 맞춰 계산한다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class BattleCameraDirector : MonoBehaviour
    {
        [Serializable]
        struct ShotCamera
        {
            public BattleShot shot;
            public CinemachineCamera camera;
        }

        [SerializeField] ShotCamera[] shots;
        [SerializeField] CinemachineImpulseSource impulse;
        [Tooltip("탄막 턴 카메라 내려다보는 각도 (도)")]
        [SerializeField, Range(30f, 80f)] float overviewPitch = 55f;
        [SerializeField, Range(20f, 70f)] float overviewFov = 40f;
        [SerializeField] float overviewMargin = 1.15f;
        [Tooltip("흔들림 1.0일 때 카메라가 밀리는 거리 (m). Impulse 기본값(약 1m)은 너무 커서 바닥 밑으로 들어감")]
        [SerializeField, Range(0.01f, 0.5f)] float shakeAmplitude = 0.06f;

        public BattleShot Current { get; private set; }
        [SerializeField] float followDistance = 2.4f;
        [SerializeField] float followHeight = 0.65f;
        [SerializeField] float shoulderOffset = 0.70f;
        [SerializeField] float followFov = 65f;
        [SerializeField] float mouseSensitivity = 0.12f;
        Transform followPlayer, followEnemy;
        InputReader actionInput;
        float yaw, followPitch = 6f;
        CinemachineCamera followCamera;
        BattleArena followArena;
        bool dialogueCameraMoved;
        Vector3 savedDialoguePosition;
        Quaternion savedDialogueRotation;
        float savedDialogueFov;
        public bool IsLockedOn { get; private set; }
        public float Yaw => yaw;
        public Vector3 FacingDirection => Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

        public void BindActions(Transform player, Transform enemy, InputReader input, BattleArena arena = null)
        {
            followPlayer = player; followEnemy = enemy; actionInput = input;
            followArena = arena;
            foreach (var shot in shots) if (shot.shot == BattleShot.Overview) followCamera = shot.camera;
            ResetFollow();
        }
        public void ResetFollow()
        {
            if (followPlayer && followEnemy)
            {
                var dir = followEnemy.position - followPlayer.position;
                yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            }
            followPitch = 6f;
        }
        public Vector2 ToWorldMove(Vector2 move)
        {
            var dir = Quaternion.Euler(0f, yaw, 0f) * new Vector3(move.x, 0f, move.y);
            return new Vector2(dir.x, dir.z);
        }
        void LateUpdate()
        {
            bool controlling = followPlayer && actionInput && actionInput.CurrentMode == InputReader.Mode.Player && Current == BattleShot.Overview && Time.timeScale > 0f;
            Cursor.lockState = controlling ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !controlling;
            if (!controlling || !followCamera)
            {
                if (followPlayer && followPlayer.TryGetComponent<PlayerMover>(out var pausedMover)) pausedMover.Motor.ClearBuffer();
                if (followArena && Current != BattleShot.Overview) followArena.UpdateBoundaryVisibility(null);
                return;
            }
            if (actionInput.ConsumeLockOn()) IsLockedOn = !IsLockedOn;
            var look = actionInput.Look;
            if (IsLockedOn && followEnemy)
            {
                var target = followEnemy.position - followPlayer.position;
                if (target.sqrMagnitude > 0.01f) yaw = Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg;
            }
            else yaw += look.x * mouseSensitivity;
            followPitch = Mathf.Clamp(followPitch - look.y * mouseSensitivity, -8f, 35f);
            var pivot = followPlayer.position + Vector3.up * followHeight;
            var rotation = Quaternion.Euler(followPitch, yaw, 0f);
            var desired = pivot - rotation * Vector3.forward * followDistance + rotation * Vector3.right * shoulderOffset;
            var ray = desired - pivot;
            if (Physics.SphereCast(pivot, 0.10f, ray.normalized, out var hit, ray.magnitude, ~0, QueryTriggerInteraction.Ignore))
                desired = pivot + ray.normalized * Mathf.Max(0.25f, hit.distance - 0.10f);
            desired.y = Mathf.Max(desired.y, followPlayer.position.y - (followPlayer.TryGetComponent<PlayerMover>(out var mover) ? mover.Motor.Height : 0f) + 0.35f);
            followCamera.transform.SetPositionAndRotation(desired, rotation);
            if (followArena) followArena.UpdateBoundaryVisibility(desired);
            var lens = followCamera.Lens; lens.FieldOfView = followFov; lens.NearClipPlane = 0.08f; followCamera.Lens = lens;
        }
        void OnDestroy() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        /// <summary>경기장 크기(가로, 세로 m)와 적 위치에 맞춰 샷을 배치한다.</summary>
        public void Setup(Vector2 arenaSize, Vector3 arenaCenter, Transform enemy)
        {
            var enemyPos = enemy ? enemy.position : arenaCenter + Vector3.forward * arenaSize.y * 0.4f;
            var enemyLook = enemyPos + Vector3.up * 0.6f;

            // Overview: 카메라 쪽(-Z)에서 내려다보기. 세로·가로 중 화면에 더 꽉 차는 쪽 기준으로 거리 계산
            float vHalf = overviewFov * 0.5f * Mathf.Deg2Rad;
            float aspect = Camera.main ? Camera.main.aspect : 16f / 9f;
            float hHalf = Mathf.Atan(Mathf.Tan(vHalf) * aspect);
            float pitch = overviewPitch * Mathf.Deg2Rad;
            float distForDepth = (arenaSize.y * 0.5f * Mathf.Sin(pitch) + 1f) / Mathf.Tan(vHalf);
            float distForWidth = arenaSize.x * 0.5f / Mathf.Tan(hHalf);
            float dist = Mathf.Max(distForDepth, distForWidth) * overviewMargin + 1f;
            var target = arenaCenter + Vector3.forward * arenaSize.y * 0.05f;
            var dir = new Vector3(0f, Mathf.Sin(pitch), -Mathf.Cos(pitch));
            Place(BattleShot.Overview, target + dir * dist, target, overviewFov);

            Place(BattleShot.EnemyFocus, enemyPos + new Vector3(0.9f, 1.3f, -3.2f), enemyLook, 35f);
            Place(BattleShot.AttackCloseUp, enemyPos + new Vector3(0.35f, 0.9f, -1.7f), enemyLook, 42f);
            Place(BattleShot.Intro, arenaCenter + new Vector3(-arenaSize.x * 0.5f - 3f, 2.2f, -arenaSize.y * 0.15f), arenaCenter + Vector3.up * 0.4f, 45f);
        }

        public void Show(BattleShot shot)
        {
            if (dialogueCameraMoved)
            {
                foreach (var camera in shots)
                    if (camera.shot == BattleShot.EnemyFocus)
                    {
                        camera.camera.transform.SetPositionAndRotation(savedDialoguePosition, savedDialogueRotation);
                        var lens = camera.camera.Lens; lens.FieldOfView = savedDialogueFov; camera.camera.Lens = lens;
                    }
                dialogueCameraMoved = false;
            }
            foreach (var s in shots) s.camera.gameObject.SetActive(s.shot == shot);
            Current = shot;
        }

        public void Shake(float force = 1f)
        {
            if (!impulse) return;
            var dir = UnityEngine.Random.insideUnitCircle.normalized;
            impulse.GenerateImpulse(new Vector3(dir.x, dir.y * 0.5f, 0f) * (force * shakeAmplitude));
        }
        public bool ShowDialogueCut(string name)
        {
            if (name == "enemy_close") Show(BattleShot.EnemyFocus);
            else if (name == "two_shot") Show(BattleShot.Intro);
            else if (name == "player_close" && followPlayer)
            {
                Show(BattleShot.EnemyFocus);
                foreach (var camera in shots)
                    if (camera.shot == BattleShot.EnemyFocus)
                    {
                        savedDialoguePosition = camera.camera.transform.position; savedDialogueRotation = camera.camera.transform.rotation;
                        savedDialogueFov = camera.camera.Lens.FieldOfView;
                    }
                var target = followPlayer.position + Vector3.up * .65f;
                Place(BattleShot.EnemyFocus, target + new Vector3(.7f, .35f, 1.8f), target, 38f);
                dialogueCameraMoved = true;
            }
            else { Debug.LogWarning("등록되지 않은 대화 카메라: " + name); return false; }
            return true;
        }

        void Place(BattleShot shot, Vector3 position, Vector3 lookAt, float fov)
        {
            foreach (var s in shots)
            {
                if (s.shot != shot) continue;
                s.camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookAt - position, Vector3.up));
                var lens = s.camera.Lens;
                lens.FieldOfView = fov;
                s.camera.Lens = lens;
            }
        }
    }
}
