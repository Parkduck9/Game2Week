using UnityEngine;

namespace Game2Week.Data
{
    [CreateAssetMenu(menuName = "Game2Week/Item", fileName = "Item_")]
    public sealed class ItemData : ScriptableObject
    {
        [SerializeField] string displayName = "아이템";
        [SerializeField, TextArea] string description;
        [SerializeField, Min(0)] int healAmount = 10;
        [Tooltip("사용했을 때 대사. {0} = 아이템 이름, {1} = 회복량")]
        [SerializeField, TextArea] string useText = "* {0}을(를) 사용했다.\n* HP가 {1} 회복되었다.";

        public string DisplayName => displayName;
        public string Description => description;
        public int HealAmount => healAmount;

        public string FormatUseText(int healed) => string.Format(useText, displayName, healed);
    }
}
