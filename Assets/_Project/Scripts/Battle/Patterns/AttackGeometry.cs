using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>상대 이동으로 탄/플레이어가 프레임 사이를 통과하는 경우도 검사한다.</summary>
    public static class AttackGeometry
    {
        public static bool SweptBody(Vector3 bulletFrom, Vector3 bulletTo, Vector3 playerFrom, Vector3 playerTo,
            float radius, float bottom, float top)
        {
            var from = bulletFrom - playerFrom;
            var to = bulletTo - playerTo;
            var delta = to - from;
            float start = 0f, end = 1f;
            if (Mathf.Abs(delta.y) < 0.00001f)
            { if (from.y < bottom || from.y > top) return false; }
            else
            {
                float a = (bottom - from.y) / delta.y, b = (top - from.y) / delta.y;
                start = Mathf.Max(0f, Mathf.Min(a, b)); end = Mathf.Min(1f, Mathf.Max(a, b));
                if (start > end) return false;
            }
            var flat = new Vector2(from.x, from.z);
            var velocity = new Vector2(delta.x, delta.z);
            float t = velocity.sqrMagnitude < 0.000001f ? start : Mathf.Clamp(-Vector2.Dot(flat, velocity) / velocity.sqrMagnitude, start, end);
            return (flat + velocity * t).sqrMagnitude <= radius * radius;
        }

        public static Vector3 DeflectionDirection(Vector3 bulletPosition, Vector3 playerPosition, Vector3 forward, Vector3 right)
        {
            // 왼쪽 탄은 오른쪽 어깨 뒤, 오른쪽 탄은 왼쪽 어깨 뒤.
            float outgoingSide = Vector3.Dot(bulletPosition - playerPosition, right) > 0f ? -1f : 1f;
            return (right * outgoingSide * 0.8f - forward).normalized;
        }
    }
}
