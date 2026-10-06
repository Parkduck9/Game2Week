using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    public interface IThreatSource
    {
        void CollectThreats(List<Vector3> positions);
    }
}
