using System.Collections.Generic;
using UnityEngine;

namespace Game2Week.Data
{
    [CreateAssetMenu(menuName = "Game2Week/Player", fileName = "Player_")]
    public sealed class PlayerData : ScriptableObject
    {
        [SerializeField] string displayName = "주인공";
        [SerializeField, Min(1)] int level = 1;
        [SerializeField, Min(1)] int maxHp = 20;
        [SerializeField, Min(0)] int attack = 10;
        [SerializeField, Min(0)] int defense;
        [SerializeField] List<ItemData> startingItems = new();

        public string DisplayName => displayName;
        public int Level => level;
        public int MaxHp => maxHp;
        public int Attack => attack;
        public int Defense => defense;
        public IReadOnlyList<ItemData> StartingItems => startingItems;
    }
}
