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
        float side=1,hitLeft,landLeft,stopLeft,lastSpeed;
        bool strong;
        PlayerMotion? dialoguePose,outcomePose,actionPose;
        bool wasAirborne,fallen,hasState;
        public PlayerMotion Current{get;private set;}
        public bool IsReady=>animator&&animator.runtimeAnimatorController;
        public Animator Animator=>animator;
        public void Bind(PlayerMover player,BattleFeedback events=null)
        {
            Unbind();mover=player;feedback=events;hasState=false;fallen=false;dialoguePose=null;outcomePose=null;actionPose=null;strong=false;hitLeft=landLeft=stopLeft=lastSpeed=0;
            if(!animator)animator=GetComponentInChildren<Animator>();
            if(animator)animator.speed=1;
            if(feedback!=null){feedback.PlayerParried+=OnParry;feedback.PlayerHit+=OnHit;}
        }
        public void SetParrySide(float value)=>side=value;
        void OnParry(Vector3 at,float value)=>SetParrySide(value);
        void OnHit(Vector3 at){if(hitLeft<=0)strong=false;hitLeft=Mathf.Max(hitLeft,.28f);}
        public void PlayDamage(int amount,int maxHp)
        {if(amount<=0)return;strong=PlayerAnimationMap.StrongHit(amount,maxHp);hitLeft=strong?.42f:.28f;}
        public bool PlayDialoguePose(string id)
        {if(!PlayerAnimationMap.TryPose(id,out var pose))return false;dialoguePose=pose;return true;}
        public void ClearDialoguePose()=>dialoguePose=null;
        public void PlayOutcome(BattleOutcome value)
        {if(value==BattleOutcome.PlayerDefeated)PlayFall();else outcomePose=value==BattleOutcome.EnemySpared?PlayerMotion.Spare:PlayerMotion.Victory;}
        public void PlayAction(ActionCue cue){dialoguePose=null;actionPose=PlayerAnimationMap.ForAction(cue);}
        public void ClearAction()=>actionPose=null;
        public void HoldAction(bool value){if(animator)animator.speed=value?0:1;}
        public void PlayFall(){fallen=true;}
        public void Configure(Animator value)=>animator=value;
        void LateUpdate()
        {
            if(!IsReady||!mover||mover.Motor==null||Time.timeScale<=0)return;
            stopLeft=Mathf.Max(0,stopLeft-Time.deltaTime);
            if(lastSpeed>.05f&&mover.GroundSpeed<=.05f)stopLeft=.18f;lastSpeed=mover.GroundSpeed;
            hitLeft=Mathf.Max(0,hitLeft-Time.deltaTime);landLeft=Mathf.Max(0,landLeft-Time.deltaTime);
            var motor=mover.Motor;if(wasAirborne&&!motor.Airborne)landLeft=.16f;wasAirborne=motor.Airborne;
            var next=PlayerAnimationMap.Resolve(mover.GroundSpeed,motor.Dodging,motor.Airborne,motor.Parrying,motor.Bracing,motor.BraceReady,side,landLeft>0,hitLeft>0,fallen);
            next=PlayerAnimationMap.Present(next,mover.IsLockOnMovement,stopLeft>0,strong,actionPose??dialoguePose,outcomePose);
            var direction=PlayerAnimationMap.LocalDirection(mover.GroundVelocity,mover.transform.rotation);
            animator.SetFloat("MoveX",direction.x);animator.SetFloat("MoveY",direction.y);
            if(!hasState||next!=Current)
            {animator.CrossFadeInFixedTime(next.ToString(),.035f);Current=next;hasState=true;}
        }
        void Unbind(){if(feedback!=null){feedback.PlayerParried-=OnParry;feedback.PlayerHit-=OnHit;}feedback=null;}
        void OnDestroy()=>Unbind();
    }
}
