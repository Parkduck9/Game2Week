using System;
using System.Collections.Generic;

namespace Game2Week.UI
{
    /// <summary>메뉴 커서 위치 계산 (끝에서 넘기면 반대쪽으로 돈다, 비활성 항목은 건너뛴다).</summary>
    public sealed class MenuList
    {
        readonly HashSet<int> disabled = new();

        public int Count { get; private set; }
        public int Index { get; private set; }

        public void SetCount(int count, IEnumerable<int> disabledIndices = null)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            Count = count;
            disabled.Clear();
            if (disabledIndices != null) disabled.UnionWith(disabledIndices);
            Index = count == 0 ? 0 : Math.Clamp(Index, 0, count - 1);
            if (!IsEnabled(Index)) Reset();
        }

        public bool IsEnabled(int index) => index >= 0 && index < Count && !disabled.Contains(index);

        /// <summary>첫 번째 선택 가능한 항목으로</summary>
        public void Reset()
        {
            Index = 0;
            for (int i = 0; i < Count; i++)
                if (IsEnabled(i)) { Index = i; return; }
        }

        /// <summary>특정 항목으로 커서 이동 (비활성이면 무시)</summary>
        public void Select(int index)
        {
            if (IsEnabled(index)) Index = index;
        }

        /// <summary>커서가 움직였으면 true.</summary>
        public bool Move(int delta)
        {
            if (Count <= 1 || delta == 0) return false;
            int step = Math.Sign(delta);
            int next = Index;
            for (int tries = 0; tries < Count; tries++)
            {
                next = ((next + step) % Count + Count) % Count;
                if (IsEnabled(next)) break;
            }
            if (next == Index || !IsEnabled(next)) return false;
            Index = next;
            return true;
        }
    }
}
