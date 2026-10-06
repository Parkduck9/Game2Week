using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>직선 진행에 수평 사인 오프셋을 더한다. 공통 피격/쳐내기를 재사용.</summary>
    public sealed class SineBullet : Bullet
    {
        [SerializeField, Min(0f)] float amplitude = 0.38f;
        [SerializeField, Min(0f)] float frequency = 0.65f;
        Vector3 origin, forward;
        float speed, elapsed;
        protected override void OnLaunch(Vector3 position, Vector3 velocity)
        { origin = position; speed = velocity.magnitude; forward = velocity.normalized; elapsed = 0f; }
        protected override void Move(float dt)
        {
            elapsed += dt;
            transform.position = WaveTrajectory.Evaluate(origin,forward,elapsed,speed,amplitude,frequency);
        }
    }
}
