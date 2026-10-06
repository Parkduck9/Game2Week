namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 탄이 몸과 겹쳤을 때 실제로 피해를 줄지 정한다 (색 규칙: 빨강 정지 자세, 파랑 이동 등).
    /// PatternContext에 없으면 겹치면 항상 맞는다.
    /// </summary>
    public interface IHitRule
    {
        bool ShouldHit(AttackColor color);
    }
}
