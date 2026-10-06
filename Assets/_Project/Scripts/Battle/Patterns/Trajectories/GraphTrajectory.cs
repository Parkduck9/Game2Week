using System;
using UnityEngine;
using Game2Week.Data.Patterns;

namespace Game2Week.Battle.Patterns.Trajectories
{
    /// <summary>Immutable launch evaluator, shared by the game and editor. Speed is integrated
    /// on a fixed lifetime grid, so the result is independent of frame subdivision.</summary>
    public abstract class GraphTrajectory : ITrajectory
    {
        readonly float[] distances = new float[257];
        protected readonly float Lifetime, Amplitude, Frequency, Radius;
        protected readonly Vector3 Control1, Control2, End;
        protected GraphTrajectory(GraphPatternDefinition d)
        {
            Lifetime=d.lifetime;Amplitude=d.amplitude;Frequency=d.frequency;Radius=d.arcRadius;
            Control1=d.bezierControl1;Control2=d.bezierControl2;End=d.bezierEnd;
            for(int i=1;i<distances.Length;i++)
            {
                float a=(i-1)/256f,b=i/256f;
                distances[i]=distances[i-1]+Lifetime/256f*(Mathf.Max(0,d.speedMultiplier.Evaluate(a))+Mathf.Max(0,d.speedMultiplier.Evaluate(b)))*.5f;
            }
        }
        public float IntegratedSeconds(float elapsed)
        {
            float index=Mathf.Clamp01(elapsed/Lifetime)*256;
            int i=Mathf.Min(255,Mathf.FloorToInt(index));
            return Mathf.Lerp(distances[i],distances[i+1],index-i);
        }
        public Vector3 Evaluate(in TrajectoryLaunch launch,float elapsed)
        {
            float travel=IntegratedSeconds(elapsed)*launch.Speed;
            float total=distances[256]*launch.Speed;
            var p=Local(travel, total>.0001f?travel/total:0);
            return launch.Origin+launch.Right*p.x+Vector3.up*p.y+launch.Forward*p.z;
        }
        protected abstract Vector3 Local(float distance,float progress);
        public static GraphTrajectory Create(GraphPatternDefinition d)
        {
            switch(d.trajectory)
            {
                case TrajectoryKind.Sine:return new SineTrajectory(d);
                case TrajectoryKind.Zigzag:return new ZigzagTrajectory(d);
                case TrajectoryKind.Parabola:return new ParabolaTrajectory(d);
                case TrajectoryKind.Arc:return new ArcTrajectory(d);
                case TrajectoryKind.Spiral:return new SpiralTrajectory(d);
                case TrajectoryKind.FigureEight:return new FigureEightTrajectory(d);
                case TrajectoryKind.Bezier:return new BezierTrajectory(d);
                default:return new StraightTrajectory(d);
            }
        }
    }
    public sealed class StraightTrajectory : GraphTrajectory
    { public StraightTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u)=>new Vector3(0,0,s); }
    public sealed class SineTrajectory : GraphTrajectory
    { public SineTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u)=>new Vector3(Amplitude*Mathf.Sin(2*Mathf.PI*Frequency*s),0,s); }
    public sealed class ZigzagTrajectory : GraphTrajectory
    { public ZigzagTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u)=>new Vector3(Amplitude*(1-4*Mathf.Abs(Mathf.Repeat(Frequency*s+.25f,1)-.5f)),0,s); }
    public sealed class ParabolaTrajectory : GraphTrajectory
    { public ParabolaTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u)=>new Vector3(0,4*Amplitude*u*(1-u),s); }
    public sealed class ArcTrajectory : GraphTrajectory
    { public ArcTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u)=>new Vector3(Radius*(1-Mathf.Cos(s/Radius)),0,Radius*Mathf.Sin(s/Radius)); }
    public sealed class SpiralTrajectory : GraphTrajectory
    { public SpiralTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u){float a=2*Mathf.PI*Frequency*s;return new Vector3(Amplitude*u*Mathf.Sin(a),0,Amplitude*u*Mathf.Cos(a));} }
    public sealed class FigureEightTrajectory : GraphTrajectory
    { public FigureEightTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u){float a=2*Mathf.PI*Frequency*s;return new Vector3(Amplitude*Mathf.Sin(a),0,Amplitude*.5f*Mathf.Sin(2*a));} }
    public sealed class BezierTrajectory : GraphTrajectory
    { public BezierTrajectory(GraphPatternDefinition d):base(d){} protected override Vector3 Local(float s,float u){float v=1-u;return 3*v*v*u*Control1+3*v*u*u*Control2+u*u*u*End;} }
}
