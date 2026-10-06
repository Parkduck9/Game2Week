using System;

namespace Game2Week.UI
{
    /// <summary>메뉴 커서 위치 계산 (끝에서 넘기면 반대쪽으로 돈다).</summary>
    public sealed class MenuList
    {
        public int Count { get; private set; }
        public int Index { get; private set; }

        public void SetCount(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            Count = count;
            Index = count == 0 ? 0 : Math.Clamp(Index, 0, count - 1);
        }

        public void Reset() => Index = 0;

        /// <summary>커서가 움직였으면 true.</summary>
        public bool Move(int delta)
        {
            if (Count <= 1 || delta == 0) return false;
            Index = ((Index + delta) % Count + Count) % Count;
            return true;
        }
    }
}
