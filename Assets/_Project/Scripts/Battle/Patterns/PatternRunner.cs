using Game2Week.Data;
using UnityEngine;

namespace Game2Week.Battle.Patterns
{
    /// <summary>탄막 턴마다 패턴 프리팹을 만들어 돌리고, 턴이 끝나면 치운다.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class PatternRunner : MonoBehaviour
    {
        GameObject instance;
        IAttackPattern pattern;

        public IAttackPattern Current => pattern;
        public GameObject CurrentObject => instance;

        public void Begin(AttackPatternData data, PatternContext context, Transform parent)
        {
            End();
            if (!data || !data.PatternPrefab) return;

            instance = Instantiate(data.PatternPrefab, parent);
            instance.name = $"Pattern_{data.name}";
            if (!instance.TryGetComponent(out pattern))
            {
                Debug.LogError($"{data.name}: 프리팹에 IAttackPattern 컴포넌트가 없음");
                Destroy(instance);
                instance = null;
                return;
            }
            pattern.Begin(context);
        }

        void Update() => pattern?.Tick(Time.deltaTime);

        public void End()
        {
            pattern?.End();
            pattern = null;
            if (instance) Destroy(instance);
            instance = null;
        }
    }
}
