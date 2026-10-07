using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 예측 조준 (사용자 요청 2026-10-07): 주인공이 지금 움직이는 방향으로, 탄이 닿을 때쯤 있을 자리를 노린다.
    /// 조준이 고정되는 예고 시간 + 비행 시간만큼 앞을 보고, lead(0~1)만큼만 반영한다 (1 = 완전 예측, 0 = 지금 위치).
    /// 순수 함수 — 게임과 Pattern Editor 미리보기가 같은 값을 쓴다.
    /// </summary>
    public static class AimLead
    {
        /// <summary>너무 먼 미래는 맞히기 어렵고 불공정하므로 이 초까지만 앞을 본다</summary>
        public const float MaxLookAheadSeconds = 2f;

        /// <param name="origin">발사 위치</param>
        /// <param name="target">주인공 현재 위치</param>
        /// <param name="velocity">주인공 수평 속도</param>
        /// <param name="bulletSpeed">탄속 (m/s)</param>
        /// <param name="delaySeconds">조준 고정 후 발사까지 남은 시간 (예고 시간)</param>
        /// <param name="lead">예측 비율 0~1</param>
        public static Vector3 Predict(Vector3 origin, Vector3 target, Vector3 velocity, float bulletSpeed, float delaySeconds, float lead)
        {
            velocity.y = 0f;
            if (lead <= 0f || velocity.sqrMagnitude < 0.0001f || bulletSpeed <= 0.01f) return target;
            // 비행 시간은 예측 지점까지 거리에 따라 다시 계산 (탄이 주인공보다 빠르면 몇 번이면 수렴)
            float t = delaySeconds + Flat(target - origin) / bulletSpeed;
            for (int i = 0; i < 3; i++)
            {
                var guess = target + velocity * Mathf.Min(t, MaxLookAheadSeconds);
                t = delaySeconds + Flat(guess - origin) / bulletSpeed;
            }
            return target + velocity * (Mathf.Min(t, MaxLookAheadSeconds) * Mathf.Clamp01(lead));
        }

        static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
    }
}
