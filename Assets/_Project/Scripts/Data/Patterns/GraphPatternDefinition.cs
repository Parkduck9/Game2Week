using UnityEngine;
using Game2Week.Battle.Patterns;

namespace Game2Week.Data.Patterns
{
    public enum TrajectoryKind { Straight, Sine, Zigzag, Parabola, Arc, Spiral, FigureEight, Bezier }
    public enum ShotLayout { Single, Fan, Ring }
    public enum ShotOrigin { Enemy, Left, Right, Alternating }

    [CreateAssetMenu(menuName = "Game2Week/Graph Pattern")]
    public sealed class GraphPatternDefinition : ScriptableObject
    {
        public TrajectoryKind trajectory;
        public AttackColor color = AttackColor.Yellow;
        public ShotLayout layout;
        public ShotOrigin origin;
        [Min(1)] public int shotCount = 1;
        [Range(0,360)] public float spreadDegrees = 45f;
        [Min(0)] public float aimRadius = .25f;
        public int seed = 2718;
        [Min(.1f)] public float warningSeconds = .85f;
        [Min(.1f)] public float lifetime = 3f;
        [Min(.1f)] public float speed = 2.4f;
        [Min(0)] public float height = .32f;
        [Min(.1f)] public float minimumInterval = .2f;
        public AnimationCurve interval = AnimationCurve.Linear(0,1.5f,30,1.5f);
        public AnimationCurve speedMultiplier = AnimationCurve.Linear(0,1,1,1);
        [Min(0)] public float amplitude = .38f;
        [Min(.01f)] public float frequency = .65f;
        [Min(.1f)] public float arcRadius = 4f;
        public Vector3 bezierControl1 = new Vector3(1,0,2);
        public Vector3 bezierControl2 = new Vector3(-1,0,4);
        public Vector3 bezierEnd = new Vector3(0,0,6);
    }
}
