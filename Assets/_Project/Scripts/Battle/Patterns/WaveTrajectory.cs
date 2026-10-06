using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    public static class WaveTrajectory
    {
        public static Vector3 Evaluate(Vector3 origin, Vector3 forward, float elapsed, float speed, float amplitude, float frequency)
        {
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            return origin + forward * (elapsed * speed) + right * (amplitude * Mathf.Sin(elapsed * frequency * Mathf.PI * 2f));
        }
        public static Vector3 AimOffset(System.Random random, float radius)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float distance = Mathf.Sqrt((float)random.NextDouble()) * radius;
            return new Vector3(Mathf.Cos(angle)*distance,0f,Mathf.Sin(angle)*distance);
        }
    }
}
