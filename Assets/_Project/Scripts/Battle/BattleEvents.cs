using System;

namespace Game2Week.Battle
{
    /// <summary>
    /// 전투 로직 → UI·연출로 가는 알림 창구. 로직은 Raise만 하고, UI·View(3D 연출)는 구독만 한다.
    /// 전투 한 번마다 새로 만든다 (씬 범위).
    /// </summary>
    public sealed class BattleEvents
    {
        public event Action<BattleStateId> StateChanged;
        /// <summary>(현재 HP, 최대 HP)</summary>
        public event Action<int, int> PlayerHpChanged;
        /// <summary>(현재 HP, 최대 HP)</summary>
        public event Action<int, int> EnemyHpChanged;
        /// <summary>받은 데미지. 0이면 MISS.</summary>
        public event Action<int> EnemyDamaged;
        public event Action<int> PlayerDamaged;
        public event Action<BattleOutcome> BattleEnded;
        /// <summary>탄막 턴 시작 때 적이 한 대사 (말풍선용, 없으면 호출 안 함)</summary>
        public event Action<string> EnemySpoke;
        public event Action<string, string, string> DialogueCue;
        public event Action<string> SpareChanged;
        public void RaiseDialogueCue(string anchor, string pose, string camera) => DialogueCue?.Invoke(anchor, pose, camera);
        public void RaiseSpareChanged(string icons) => SpareChanged?.Invoke(icons);

        public void RaiseEnemySpoke(string line) => EnemySpoke?.Invoke(line);

        public void RaiseStateChanged(BattleStateId id) => StateChanged?.Invoke(id);

        public void RaisePlayerHpChanged(int current, int max) => PlayerHpChanged?.Invoke(current, max);

        public void RaiseEnemyHpChanged(int current, int max) => EnemyHpChanged?.Invoke(current, max);

        public void RaiseEnemyDamaged(int amount) => EnemyDamaged?.Invoke(amount);

        public void RaisePlayerDamaged(int amount) => PlayerDamaged?.Invoke(amount);

        public void RaiseBattleEnded(BattleOutcome outcome) => BattleEnded?.Invoke(outcome);
    }
}
