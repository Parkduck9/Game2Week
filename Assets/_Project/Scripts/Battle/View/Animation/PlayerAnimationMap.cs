namespace Game2Week.Battle.View.Animation
{
    public enum PlayerMotion { Idle, Run, Dodge, Jump, Land, ParryLeft, ParryRight, Brace, Hit, Fall }
    /// <summary>판정과 무관한 표현 우선순위. 실제 이동 속도를 사용한다.</summary>
    public static class PlayerAnimationMap
    {
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
