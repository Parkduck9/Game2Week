using System;
using UnityEngine;

namespace Game2Week.Battle
{
    public enum ActionCue { Talk, Cheer, Play, RunTogether, Strike, Retreat }
    public interface IActionPresentationWorld
    {
        float PresentationDuration(ActionCue cue);
        float StrikeImpactTime {get;}
        float StrikeHoldSeconds {get;}
        void BeginPresentation(ActionCue cue);
        void UpdatePresentation(ActionCue cue,float progress,bool hold);
        void EndPresentation(ActionCue cue);
    }
    public sealed class ActionPresentationRequest
    {
        public ActionCue cue;public Action impact,finished;public bool pauseImpact;
    }
    /// <summary>영향 시점을 한 번만 통과하고 짧은 국소 정지를 포함하는 연출 시계.</summary>
    public sealed class ActionPresentationClock
    {
        readonly float duration,impactAt,holdDuration;
        float elapsed,holdLeft;bool impacted;
        public ActionPresentationClock(float duration,float impactAt,float holdDuration)
        {this.duration=Mathf.Max(0,duration);this.impactAt=Mathf.Clamp(impactAt,0,this.duration);this.holdDuration=Mathf.Max(0,holdDuration);}
        public float Progress=>duration>0?Mathf.Clamp01(elapsed/duration):1;
        public bool Holding=>holdLeft>0;
        public bool Complete=>impacted&&elapsed>=duration&&holdLeft<=0;
        public bool Advance(float dt)
        {
            if(dt<=0)return false;bool impactNow=false;
            if(!impacted&&elapsed+dt>=impactAt)
            {dt-=Mathf.Max(0,impactAt-elapsed);elapsed=impactAt;impacted=true;holdLeft=holdDuration;impactNow=true;}
            if(holdLeft>0){float used=Mathf.Min(dt,holdLeft);holdLeft-=used;dt-=used;}
            elapsed=Mathf.Min(duration,elapsed+dt);return impactNow;
        }
    }
    public static class RetreatPath
    {
        /// <summary>후퇴 뒤 원하는 둘 사이 거리 = max(최소 거리, 처음 시작 거리 × 비율)</summary>
        public static float DesiredSeparation(float minimum,float startDistance,float ratio)=>Mathf.Max(minimum,startDistance*ratio);
        /// <summary>이만큼 떨어져 있으면 후퇴하지 않는다 (원하는 거리의 90%)</summary>
        public static bool FarEnough(float separation,float desired)=>separation>=desired*.9f;
        /// <summary>후퇴 시간 = 이동 거리 / 속도, [최소, 최대]</summary>
        public static float Duration(float travel,float speed,float min,float max)=>Mathf.Clamp(travel/Mathf.Max(.1f,speed),min,Mathf.Max(min,max));
        public static Vector3 Target(Vector3 enemy,Vector3 player,float distance,Func<Vector3,Vector3> clamp)
        {
            var away=enemy-player;away.y=0;if(away.sqrMagnitude<.0001f)away=Vector3.forward;away.Normalize();
            var best=enemy;float bestScore=-1;
            foreach(float angle in new[]{0f,45f,-45f,90f,-90f,135f,-135f,180f})
            {
                var candidate=clamp(enemy+Quaternion.Euler(0,angle,0)*away*distance);
                float separation=(candidate-player).sqrMagnitude;
                if(angle==0&&(candidate-enemy).sqrMagnitude>=distance*distance*.8f)return candidate;
                if(separation>bestScore){best=candidate;bestScore=separation;}
            }
            return best;
        }
    }
}
