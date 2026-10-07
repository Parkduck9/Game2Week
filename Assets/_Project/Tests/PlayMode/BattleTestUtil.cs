using Game2Week.Flow;
using UnityEngine;

namespace Game2Week.Tests
{
    /// <summary>
    /// 9단계 이후 공용 도움: 모든 맵이 PatternDirector(겹)로 돌아가고 맵이 넓어졌으므로
    /// 하위 패턴은 자식에서 찾고, 탄을 맞히는 시험은 주인공을 적 가까이 옮긴다.
    /// </summary>
    public static class BattleTestUtil
    {
        /// <summary>지금 탄막(Director 아래 자식 포함)에서 T 패턴 하나</summary>
        public static T FindPattern<T>(BattleController battle) where T : Component =>
            battle.World.Patterns.CurrentObject ? battle.World.Patterns.CurrentObject.GetComponentInChildren<T>() : null;

        /// <summary>주인공을 시작점 쪽에서 적까지 meters만 남기고 옮긴다 (탄이 금방 닿게)</summary>
        public static void MoveNearEnemy(BattleController battle, float meters)
        {
            var player = battle.World.Player;
            var enemy = battle.Spawner.Enemy.transform.position;
            var away = player.transform.position - enemy; away.y = 0f;
            player.Teleport(enemy + away.normalized * meters);
        }
    }
}
