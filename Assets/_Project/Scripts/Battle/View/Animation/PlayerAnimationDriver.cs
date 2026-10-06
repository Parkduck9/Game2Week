using UnityEngine;

namespace Game2Week.Battle.View.Animation
{
    /// <summary>모터 상태를 읽고 Animator만 구동한다. 피격 사건은 Bind로 주입한다.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        [SerializeField] Animator animator;
        PlayerMover mover;
        BattleFeedback feedback;
        float side=1,hitLeft,landLeft;
        bool wasAirborne,fallen,hasState;
        public PlayerMotion Current{get;private set;}
        public bool IsReady=>animator&&animator.runtimeAnimatorController;
        public Animator Animator=>animator;
        public void Bind(PlayerMover player,BattleFeedback events=null)
        {
            Unbind();mover=player;feedback=events;hasState=false;
            if(!animator)animator=GetComponentInChildren<Animator>();
            if(feedback!=null){feedback.PlayerParried+=OnParry;feedback.PlayerHit+=OnHit;}
        }
        public void SetParrySide(float value)=>side=value;
        void OnParry(Vector3 at,float value)=>SetParrySide(value);
        void OnHit(Vector3 at)=>hitLeft=.28f;
        public void PlayFall(){fallen=true;}
        public void Configure(Animator value)=>animator=value;
        void LateUpdate()
        {
            if(!IsReady||!mover||mover.Motor==null||Time.timeScale<=0)return;
            hitLeft=Mathf.Max(0,hitLeft-Time.deltaTime);landLeft=Mathf.Max(0,landLeft-Time.deltaTime);
            var motor=mover.Motor;if(wasAirborne&&!motor.Airborne)landLeft=.16f;wasAirborne=motor.Airborne;
            var next=PlayerAnimationMap.Resolve(mover.GroundSpeed,motor.Dodging,motor.Airborne,motor.Parrying,motor.Bracing,motor.BraceReady,side,landLeft>0,hitLeft>0,fallen);
            if(!hasState||next!=Current)
            {animator.CrossFadeInFixedTime(next.ToString(),.035f);Current=next;hasState=true;}
        }
        void Unbind(){if(feedback!=null){feedback.PlayerParried-=OnParry;feedback.PlayerHit-=OnHit;}feedback=null;}
        void OnDestroy()=>Unbind();
    }
}
