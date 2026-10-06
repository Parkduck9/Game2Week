using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Game2Week.Battle.View
{
    public enum BattleShot
    {
        /// <summary>전투 시작 — 경기장을 옆에서 넓게</summary>
        Intro,
        /// <summary>탄막 턴 — 비스듬히 내려다보는 고정 카메라 (경기장 전체)</summary>
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
            foreach (var s in shots) s.camera.gameObject.SetActive(s.shot == shot);
            Current = shot;
        }

        public void Shake(float force = 1f)
        {
            if (!impulse) return;
            var dir = UnityEngine.Random.insideUnitCircle.normalized;
            impulse.GenerateImpulse(new Vector3(dir.x, dir.y * 0.5f, 0f) * (force * shakeAmplitude));
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
