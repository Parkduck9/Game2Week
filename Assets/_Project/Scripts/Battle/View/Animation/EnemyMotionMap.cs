namespace Game2Week.Battle.View.Animation
{
    public enum EnemyMotion { Idle, Warning, Fire, Hit, Phase, Relaxed, Spared, Defeated, Talk, Nod, Surprise }
    public static class EnemyMotionMap
    {
        public static EnemyMotion Resolve(bool defeated,bool spared,bool hit,bool phase,bool warning,bool fire,bool relaxed,EnemyMotion? pose)
        {
            if(defeated)return EnemyMotion.Defeated;if(spared)return EnemyMotion.Spared;
            if(hit)return EnemyMotion.Hit;if(phase)return EnemyMotion.Phase;
            if(warning)return EnemyMotion.Warning;if(fire)return EnemyMotion.Fire;
            if(pose.HasValue)return pose.Value;
            return relaxed?EnemyMotion.Relaxed:EnemyMotion.Idle;
        }
        public static bool TryPose(string id,out EnemyMotion motion)
        {
            switch((id??string.Empty).ToLowerInvariant())
            {
                case "idle":motion=EnemyMotion.Idle;return true;
                case "talk":motion=EnemyMotion.Talk;return true;
                case "nod":motion=EnemyMotion.Nod;return true;
                case "surprise":motion=EnemyMotion.Surprise;return true;
                case "spare":motion=EnemyMotion.Relaxed;return true;
                case "victory":motion=EnemyMotion.Talk;return true;
                default:motion=EnemyMotion.Idle;return false;
            }
        }
    }
}
