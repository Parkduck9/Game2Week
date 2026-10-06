using System;
using System.Collections.Generic;

namespace Game2Week.Battle.View
{
    /// <summary>이펙트 재사용 목록 (순수 로직) — 쉬고 있는 것을 먼저 쓰고, 없을 때만 새로 만든다.</summary>
    public sealed class FxPool<T> where T : class
    {
        readonly Func<T> create;
        readonly Func<T, bool> isFree;
        readonly List<T> items = new();

        public FxPool(Func<T> create, Func<T, bool> isFree)
        {
            this.create = create;
            this.isFree = isFree;
        }

        public int Count => items.Count;
        public IReadOnlyList<T> Items => items;

        public T Get()
        {
            foreach (var item in items) if (isFree(item)) return item;
            var created = create();
            items.Add(created);
            return created;
        }
    }
}
