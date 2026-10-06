using System;
using System.Collections.Generic;
using Game2Week.Data;

namespace Game2Week.Battle
{
    public sealed class Inventory
    {
        readonly List<ItemData> items = new();

        public Inventory(IEnumerable<ItemData> startingItems = null)
        {
            if (startingItems == null) return;
            foreach (var item in startingItems)
                if (item) items.Add(item);
        }

        public IReadOnlyList<ItemData> Items => items;
        public bool IsEmpty => items.Count == 0;

        /// <summary>해당 칸의 아이템을 꺼낸다 (소모).</summary>
        public ItemData Take(int index)
        {
            if (index < 0 || index >= items.Count) throw new ArgumentOutOfRangeException(nameof(index));
            var item = items[index];
            items.RemoveAt(index);
            return item;
        }
    }
}
