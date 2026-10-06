using System;
using System.Collections.Generic;
using UnityEngine;
using Game2Week.Data.Patterns;

namespace Game2Week.Battle.Patterns.Trajectories
{
    public static class PatternGraphRules
    {
        public static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
        public static float Interval(GraphPatternDefinition d,float time)=>Mathf.Max(d.minimumInterval,d.warningSeconds,d.interval.Evaluate(time));
        public static List<string> Validate(GraphPatternDefinition d)
        {
            var errors=new List<string>();
            if(!d){errors.Add("패턴 정의가 없습니다.");return errors;}
            if(!Enum.IsDefined(typeof(TrajectoryKind),d.trajectory)||!Enum.IsDefined(typeof(ShotLayout),d.layout)||!Enum.IsDefined(typeof(ShotOrigin),d.origin)||!Enum.IsDefined(typeof(AttackColor),d.color))errors.Add("알 수 없는 종류입니다.");
            if(d.shotCount<1||d.shotCount>32)errors.Add("발사 수는 1~32개여야 합니다.");
            Check(d.warningSeconds,.1f,10,"예고",errors);Check(d.lifetime,.1f,20,"수명",errors);
            Check(d.speed,.1f,30,"기준 속도",errors);Check(d.minimumInterval,.1f,20,"최소 간격",errors);
            Check(d.height,0,10,"높이",errors);Check(d.aimRadius,0,5,"조준 오차",errors);
            Check(d.amplitude,0,10,"진폭",errors);Check(d.frequency,.01f,5,"주파수",errors);
            Check(d.arcRadius,.1f,30,"원호 반경",errors);Check(d.spreadDegrees,0,360,"배열 각도",errors);
            foreach(var p in new[]{d.bezierControl1,d.bezierControl2,d.bezierEnd})
                if(!Finite(p.x)||!Finite(p.y)||!Finite(p.z)||p.magnitude>50)errors.Add("베지어 좌표는 유한하고 50m 이내여야 합니다.");
            Curve(d.interval,0,30,.01f,30,"발사 간격",errors);
            Curve(d.speedMultiplier,0,1,0,5,"속도 배율",errors);
            return errors;
        }
        static void Check(float v,float min,float max,string label,List<string> errors)
        {if(!Finite(v)||v<min||v>max)errors.Add($"{label}: {min}~{max} 범위여야 합니다.");}
        static void Curve(AnimationCurve curve,float from,float to,float min,float max,string label,List<string> errors)
        {
            if(curve==null||curve.length==0){errors.Add(label+" 그래프가 비어 있습니다.");return;}
            foreach(var k in curve.keys) if(!Finite(k.time)||!Finite(k.value)||float.IsNaN(k.inTangent)||float.IsNaN(k.outTangent)){errors.Add(label+" 그래프 값이 유효하지 않습니다.");return;}
            for(int i=0;i<=256;i++){float value=curve.Evaluate(Mathf.Lerp(from,to,i/256f));if(!Finite(value)||value<min||value>max){errors.Add(label+" 그래프가 허용 범위를 벗어납니다.");break;}}
        }
    }
}
