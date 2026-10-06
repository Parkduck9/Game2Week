using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>임시 이펙트 (파티클 + 카메라 흔들림). 리소스가 생기면 프리팹만 바꾼다.</summary>
    public sealed class BattleEffects : MonoBehaviour
    {
        [SerializeField] ParticleSystem hitBurstPrefab;
        [SerializeField] ParticleSystem gemPickupPrefab;
        [SerializeField] BattleCameraDirector cameraDirector;
        [Tooltip("피격 파티클 크기 배율 — 공격 클로즈업에서 화면을 덮지 않게")]
        [SerializeField, Range(0.1f, 1f)] float hitSizeScale = 0.5f;
        [Tooltip("피격 파티클을 적 중심에서 카메라 쪽(적 표면)으로 당기는 거리 (m)")]
        [SerializeField, Min(0f)] float hitTowardCamera = 0.35f;

        public void PlayHit(Vector3 position, float strength = 1f)
        {
            var cam = Camera.main;
            if (cam) position += (cam.transform.position - position).normalized * hitTowardCamera;
            Spawn(hitBurstPrefab, position, hitSizeScale);
            cameraDirector.Shake(strength);
        }

        public void PlayGemPickup(Vector3 position) => Spawn(gemPickupPrefab, position, 1f);

        static void Spawn(ParticleSystem prefab, Vector3 position, float sizeScale)
        {
            if (!prefab) return;
            var ps = Instantiate(prefab, position, Quaternion.identity);
            var main = ps.main;
            main.startSizeMultiplier *= sizeScale;
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 0.2f);
        }
    }
}
