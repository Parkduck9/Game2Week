using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>경고음·화면 밖 표시용으로 지금 위험한 위치(예고된 발사원, 날아오는 탄)를 알려 준다.</summary>
    public interface IThreatSource
    {
        void CollectThreats(List<ThreatPoint> threats);
    }

    public readonly struct ThreatPoint
    {
        public ThreatPoint(Vector3 position, AttackColor color)
        {
            Position = position;
            Color = color;
        }

        public Vector3 Position { get; }
        public AttackColor Color { get; }
    }
}
