namespace Game2Week.Battle.Patterns
{
    /// <summary>
    /// PatternDirector가 하위 패턴으로 돌릴 수 있는 패턴. Begin 전에 난이도 배율을 한 번 받고, 대표 색을 알려 준다
    /// (색 조합 규칙 — 빨강·파랑 패턴은 같은 턴에 겹치지 않게).
    /// </summary>
    public interface IDirectablePattern
    {
        AttackColor PatternColor { get; }

        /// <summary>발사 간격 × intervalScale, 탄속 × speedScale. Begin 전에 한 번만 호출된다.</summary>
        void ApplyDifficulty(float intervalScale, float speedScale);
    }
}
