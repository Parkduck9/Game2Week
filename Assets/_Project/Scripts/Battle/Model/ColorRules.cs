using Game2Week.Battle.Patterns;
using Game2Week.Data;

namespace Game2Week.Battle
{
    /// <summary>
    /// 색 대응 규칙 (순수 로직). 몸과 겹친 탄을 통과시키는지 정한다.
    /// 노랑은 통과 없음 (쳐내기·회피·점프로 대응), 빨강은 정지 자세 + 실제 정지, 파랑은 실제 이동.
    /// 정지 자세는 빨강에만 효과가 있어 만능 방어가 되지 않는다.
    /// </summary>
    public static class ColorRules
    {
        public static bool Passes(AttackColor color, bool braceReady, float groundSpeed, PlayerActionSettings settings) =>
            color switch
            {
                AttackColor.Red => braceReady && groundSpeed <= settings.stillSpeedMax,
                AttackColor.Blue => groundSpeed >= settings.blueMinSpeed,
                _ => false,
            };
    }
}
