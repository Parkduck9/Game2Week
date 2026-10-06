using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>임시 이펙트 (파티클 + 카메라 흔들림). 리소스가 생기면 프리팹만 바꾼다.</summary>
    public sealed class BattleEffects : MonoBehaviour
    {
        [SerializeField] ParticleSystem hitBurstPrefab;
        [SerializeField] ParticleSystem gemPickupPrefab;
        [SerializeField] BattleCameraDirector cameraDirector;

        public void PlayHit(Vector3 position, float strength = 1f)
        {
            Spawn(hitBurstPrefab, position);
            cameraDirector.Shake(strength);
        }

        public void PlayGemPickup(Vector3 position) => Spawn(gemPickupPrefab, position);

        static void Spawn(ParticleSystem prefab, Vector3 position)
        {
            if (!prefab) return;
            var ps = Instantiate(prefab, position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 0.2f);
        }
    }
}
