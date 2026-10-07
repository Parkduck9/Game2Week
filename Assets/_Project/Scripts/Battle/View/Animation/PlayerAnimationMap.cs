namespace Game2Week.Battle.View.Animation
{
    public enum PlayerMotion { Idle, Run, Dodge, Jump, Land, ParryLeft, ParryRight, Brace, Hit, Fall, Strafe, Stop, HitStrong, Victory, Spare, Talk, Nod, Surprise }
    /// <summary>판정과 무관한 표현 우선순위. 실제 이동 속도를 사용한다.</summary>
    public static class PlayerAnimationMap
    {
        public static bool TryPose(string id,out PlayerMotion motion)
        {
            switch((id??string.Empty).ToLowerInvariant())
            {
                case "idle":motion=PlayerMotion.Idle;return true;
                case "talk":motion=PlayerMotion.Talk;return true;
                case "nod":motion=PlayerMotion.Nod;return true;
                case "surprise":motion=PlayerMotion.Surprise;return true;
                case "victory":motion=PlayerMotion.Victory;return true;
                case "spare":motion=PlayerMotion.Spare;return true;
                default:motion=PlayerMotion.Idle;return false;
            }
        }
        public static bool StrongHit(int amount,int maxHp)=>maxHp>0&&amount*5L>=maxHp;
        public static PlayerMotion Present(PlayerMotion basis,bool locked,bool stopped,bool strong,PlayerMotion? pose,PlayerMotion? outcome)
        {
            if(basis==PlayerMotion.Fall)return basis;
            if(basis==PlayerMotion.Hit)return strong?PlayerMotion.HitStrong:basis;
            if(pose.HasValue)return pose.Value;
            if(outcome.HasValue)return outcome.Value;
            if(basis==PlayerMotion.Run&&locked)return PlayerMotion.Strafe;
            if(basis==PlayerMotion.Idle&&stopped)return PlayerMotion.Stop;
            return basis;
        }
        public static UnityEngine.Vector2 LocalDirection(UnityEngine.Vector3 velocity,UnityEngine.Quaternion rotation)
        {
            var local=UnityEngine.Quaternion.Inverse(rotation)*velocity;
            return new UnityEngine.Vector2(local.x,local.z).normalized;
        }
        public static PlayerMotion Resolve(float speed,bool dodging,bool airborne,bool parrying,bool bracing,bool ready,float side,bool landed,bool hit,bool fallen)
        {
            if(fallen)return PlayerMotion.Fall;
            if(hit)return PlayerMotion.Hit;
            if(dodging)return PlayerMotion.Dodge;
            if(airborne)return PlayerMotion.Jump;
            if(parrying)return side>=0?PlayerMotion.ParryLeft:PlayerMotion.ParryRight;
            if(bracing||ready)return PlayerMotion.Brace;
            if(landed)return PlayerMotion.Land;
            return speed>.05f?PlayerMotion.Run:PlayerMotion.Idle;
        }
    }
}
