namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// 공격 색 = 대응 규칙. 노랑: 쳐내기/회피/점프, 빨강: 정지 자세로 통과, 파랑: 움직여서 통과.
    /// 궤적(ITrajectory)과는 별개로 조합한다.
    /// </summary>
    public enum AttackColor
    {
        Yellow,
        Red,
        Blue,
    }
}
