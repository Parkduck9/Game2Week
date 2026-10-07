using Game2Week.Battle.Patterns;
using UnityEngine;

namespace Game2Week.Battle.View
{
    /// <summary>전투 사건을 적의 절차 동작에 연결한다. 판정과 수치를 바꾸지 않는다.</summary>
    public sealed class EnemyMotionFxModule:MonoBehaviour,IBattleFxModule
    {
        BattleFxRig rig;
        public void Bind(BattleFxRig value)
        {
            Unbind();rig=value;
            rig.Feedback.WarningStarted+=Warning;rig.Feedback.BulletFired+=Fire;rig.Feedback.EnemyPhaseChanged+=Phase;
            rig.Events.SpareChanged+=Spare;rig.Events.StateChanged+=State;
        }
        void Warning(AttackColor color,Vector3 at){if(rig.Spawner.Enemy)rig.Spawner.Enemy.PlayWarning();}
        void Fire(AttackColor color,Vector3 at){if(rig.Spawner.Enemy)rig.Spawner.Enemy.PlayAttack();}
        void Phase(int value){if(rig.Spawner.Enemy)rig.Spawner.Enemy.PlayPhase();}
        void Spare(string icons){if(rig.Spawner.Enemy)rig.Spawner.Enemy.SetRelaxed(!string.IsNullOrEmpty(icons)&&icons.Contains("●")&&!icons.Contains("○"));}
        void State(BattleStateId value){if(rig.Spawner.Enemy)rig.Spawner.Enemy.HoldForDialogue(value==BattleStateId.Dialogue);}
        void Unbind()
        {
            if(rig==null)return;
            rig.Feedback.WarningStarted-=Warning;rig.Feedback.BulletFired-=Fire;rig.Feedback.EnemyPhaseChanged-=Phase;
            rig.Events.SpareChanged-=Spare;rig.Events.StateChanged-=State;rig=null;
        }
        void OnDestroy()=>Unbind();
    }
}
