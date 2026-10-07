using Game2Week.Data;
using UnityEngine;
namespace Game2Week.Battle.View
{
    /// <summary>탄막 중 실제 수평 위치를 완만하게 이동한다. 메뉴/행동/후퇴 중에는 멈춘다.</summary>
    public sealed class EnemyFlight:MonoBehaviour
    {
        BattleArena arena;Transform player;CombatPresentationSettings settings;Vector3 anchor;float time;bool active;
        public void Configure(BattleArena value,Transform target,CombatPresentationSettings profile)
        {arena=value;player=target;settings=profile;anchor=transform.position;}
        public void Begin(){anchor=transform.position;time=0;active=true;}
        public void Pause()=>active=false;
        void Update()
        {
            if(!active||!settings||!arena||Time.deltaTime<=0)return;
            time+=Time.deltaTime*settings.flightSpeed;
            var offset=new Vector3(Mathf.Sin(time),0,(Mathf.Cos(time)-1)*.5f)*settings.flightRadius;
            transform.position=arena.ClampToArena(anchor+offset,.7f);
            if(player){var direction=player.position-transform.position;direction.y=0;if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(direction);}
        }
    }
}
