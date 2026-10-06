using System.Collections.Generic;

namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 전투 한 번 동안 유지되는 패턴 기록 (5단계 공용). 턴마다 패턴 프리팹은 새로 만들어지므로
    /// "새 패턴을 소개했는지", 셔플 백 순서 같은 턴 간 상태는 여기에 둔다.
    /// BattleWorld가 전투 시작 때 하나 만들어 모든 PatternContext에 넣는다. 세이브하지 않으며 재도전하면 새로 시작.
    /// </summary>
    public sealed class EncounterMemory
    {
        readonly Dictionary<string, object> entries = new();

        /// <summary>key로 저장된 상태를 꺼내고, 없으면 새로 만든다.</summary>
        public T GetOrCreate<T>(string key) where T : class, new()
        {
            if (entries.TryGetValue(key, out var value) && value is T typed) return typed;
            var created = new T();
            entries[key] = created;
            return created;
        }

        public void Clear() => entries.Clear();
    }
}
