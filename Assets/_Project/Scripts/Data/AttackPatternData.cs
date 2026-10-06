using UnityEngine;

namespace Game2Week.Data
{
    /// <summary>
    /// 탄막 패턴 하나. 실제 동작은 프리팹(IAttackPattern 구현 컴포넌트)이 담당하고,
    /// 이 에셋은 이름과 프리팹을 묶는다. 탄막 턴 길이는 스테이지(enemyTurn.duration)가 정한다.
    /// 새 패턴 = IAttackPattern을 구현한 프리팹 + 이 에셋 하나.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Attack Pattern", fileName = "Pattern_")]
    public sealed class AttackPatternData : ScriptableObject
    {
        [SerializeField] string displayName = "패턴";
        [Tooltip("IAttackPattern을 구현한 컴포넌트가 붙은 프리팹")]
        [SerializeField] GameObject patternPrefab;

        public string DisplayName => displayName;
        public GameObject PatternPrefab => patternPrefab;
    }
}
