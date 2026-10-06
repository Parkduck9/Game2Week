using System.Collections.Generic;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 색 조합 규칙 (5단계 공용). 빨강(멈추기)과 파랑(움직이기)은 서로 반대 대응이라
    /// 동시에 위험한 상태로 겹치면 피할 방법이 없다 → 겹침 금지, 전환 사이에 유예를 둔다.
    /// 노랑은 어떤 색과도 겹칠 수 있다.
    /// </summary>
    public static class ColorCombinationRules
    {
        /// <summary>빨강 ↔ 파랑 위험이 바뀔 때 비워 둘 최소 시간 (초) — 자세를 풀고/잡을 시간</summary>
        public const float SwitchGraceSeconds = 0.6f;

        public static bool CanOverlap(AttackColor a, AttackColor b) =>
            !(a == AttackColor.Red && b == AttackColor.Blue) && !(a == AttackColor.Blue && b == AttackColor.Red);

        /// <summary>동시에 살아 있는 위험 색 목록이 허용되는지</summary>
        public static bool IsAllowed(IEnumerable<AttackColor> active)
        {
            bool red = false, blue = false;
            foreach (var c in active)
            {
                red |= c == AttackColor.Red;
                blue |= c == AttackColor.Blue;
            }
            return !(red && blue);
        }
    }
}
