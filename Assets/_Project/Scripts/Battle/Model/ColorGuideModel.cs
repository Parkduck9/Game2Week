using System.Collections.Generic;
using Game2Week.Battle.Patterns;

namespace Game2Week.Battle
{
    /// <summary>
    /// 한 전투에서 빨강·파랑이 처음 나온 순간을 골라낸다 (순수 로직). 노랑은 조작 안내에 이미 있어 안내하지 않는다.
    /// 전투마다 새로 만든다 — 같은 색은 한 전투에 한 번만 안내.
    /// </summary>
    public sealed class ColorGuideModel
    {
        readonly HashSet<AttackColor> seen = new();

        /// <summary>이번에 처음 본 안내 대상 색이 있으면 그 색 (없으면 null). 한 번에 하나씩, 빨강 먼저.</summary>
        public AttackColor? Observe(IEnumerable<AttackColor> visible)
        {
            bool red = false, blue = false;
            foreach (var c in visible)
            {
                red |= c == AttackColor.Red;
                blue |= c == AttackColor.Blue;
            }
            if (red && seen.Add(AttackColor.Red)) return AttackColor.Red;
            if (blue && seen.Add(AttackColor.Blue)) return AttackColor.Blue;
            return null;
        }

        public bool HasSeen(AttackColor color) => seen.Contains(color);
    }
}
