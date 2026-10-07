using System;
using System.Collections.Generic;
using UnityEngine;
using Game2Week.Data.Patterns;

namespace Game2Week.Battle.Patterns.Trajectories
{
    public readonly struct PatternShot
    {
        public PatternShot(float time,Vector3 origin,Vector3 velocity){Time=time;Origin=origin;Velocity=velocity;}
        public float Time{get;} public Vector3 Origin{get;} public Vector3 Velocity{get;}
    }
    /// <summary>Warning, seeded aim and shot schedule shared with editor simulation.
    /// The interval is evaluated once when reserving the next burst.</summary>
    public sealed class PatternTimeline
    {
        readonly GraphPatternDefinition d;
        readonly System.Random random;
        float nextShot, nextWarning;
        int burst;
        Vector3 fixedOrigin,fixedTarget;
        public float Time{get;private set;}
        public bool WarningActive{get;private set;}
        public Vector3 WarningOrigin=>fixedOrigin;
        public Vector3 WarningTarget=>fixedTarget;
        public float NextShotTime=>nextShot;
        public PatternTimeline(GraphPatternDefinition definition)
        {d=definition;random=new System.Random(d.seed);nextShot=d.warningSeconds;nextWarning=0;}
        /// <param name="targetVelocity">주인공 속도 (경기장 로컬). 미리보기처럼 모르면 0 → 예측 없음</param>
        public void Advance(float delta,Vector3 enemy,Vector3 target,Vector2 arena,List<PatternShot> output,Vector3 targetVelocity=default)
        {
            if(delta<0||!PatternGraphRules.Finite(delta))throw new ArgumentOutOfRangeException(nameof(delta));
            float end=Time+delta;
            // Bounded by the validated minimum interval, no dropped bursts during scrubbing.
            while(true)
            {
                if(!WarningActive&&nextWarning<=end+.00001f)
                {
                    var origin=d.origin==ShotOrigin.Alternating?(ShotOrigin)(burst%3):d.origin;
                    fixedOrigin=enemy;
                    if(origin==ShotOrigin.Left||origin==ShotOrigin.Right)
                        fixedOrigin=new Vector3((origin==ShotOrigin.Left?-1:1)*(arena.x*.5f-.45f),0,Mathf.Clamp(target.z+1.2f,-arena.y*.5f+.5f,arena.y*.5f-.5f));
                    fixedOrigin.y=d.height;
                    var predicted=AimLead.Predict(fixedOrigin,target,targetVelocity,d.speed,d.warningSeconds,d.leadFactor);
                    fixedTarget=predicted+WaveTrajectory.AimOffset(random,d.aimRadius);fixedTarget.y=d.height;
                    fixedTarget.x=Mathf.Clamp(fixedTarget.x,-arena.x*.5f+.35f,arena.x*.5f-.35f);
                    fixedTarget.z=Mathf.Clamp(fixedTarget.z,-arena.y*.5f+.35f,arena.y*.5f-.35f);
                    WarningActive=true;
                }
                if(!WarningActive||nextShot>end+.00001f)break;
                var direction=fixedTarget-fixedOrigin;direction.y=0;
                if(direction.sqrMagnitude<.0001f)direction=Vector3.back;
                direction.Normalize();
                int count=d.layout==ShotLayout.Single?1:d.shotCount;
                for(int i=0;i<count;i++)
                {
                    float angle=d.layout==ShotLayout.Ring?360f*i/count:count==1?0:Mathf.Lerp(-d.spreadDegrees*.5f,d.spreadDegrees*.5f,i/(count-1f));
                    output.Add(new PatternShot(nextShot,fixedOrigin,Quaternion.AngleAxis(angle,Vector3.up)*direction*d.speed));
                }
                float reserved=PatternGraphRules.Interval(d,nextShot);
                nextShot+=reserved;nextWarning=nextShot-d.warningSeconds;
                WarningActive=false;burst++;
            }
            Time=end;
        }
        public void WarningShots(List<PatternShot> output)
        {
            if(!WarningActive)return;
            var direction=fixedTarget-fixedOrigin;direction.y=0;
            if(direction.sqrMagnitude<.0001f)direction=Vector3.back;
            int count=d.layout==ShotLayout.Single?1:d.shotCount;
            for(int i=0;i<count;i++)
            {
                float angle=d.layout==ShotLayout.Ring?360f*i/count:count==1?0:Mathf.Lerp(-d.spreadDegrees*.5f,d.spreadDegrees*.5f,i/(count-1f));
                output.Add(new PatternShot(nextShot,fixedOrigin,Quaternion.AngleAxis(angle,Vector3.up)*direction.normalized*d.speed));
            }
        }
    }
}
