using UnityEngine;

namespace Game2Week.Data
{
    /// <summary>
    /// 적 턴 하나 분량의 탄막 패턴. 실제 동작은 프리팹이 담당하고 (09단계에서 IAttackPattern으로 연결),
    /// 이 에셋은 프리팹과 실행 조건만 묶는다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game2Week/Attack Pattern", fileName = "Pattern_")]
    public sealed class AttackPatternData : ScriptableObject
    {
        [SerializeField] string displayName = "패턴";
        [Tooltip("패턴 동작 프리팹 (09단계: IAttackPattern 구현 컴포넌트 필요)")]
        [SerializeField] GameObject patternPrefab;
        [SerializeField, Min(0.5f)] float duration = 6f;
        [Tooltip("이 패턴 동안의 전투 박스 크기 (박스 로컬 단위)")]
        [SerializeField] Vector2 boxSize = new(1.6f, 1.6f);

        public string DisplayName => displayName;
        public GameObject PatternPrefab => patternPrefab;
        public float Duration => duration;
        public Vector2 BoxSize => boxSize;
    }
}
