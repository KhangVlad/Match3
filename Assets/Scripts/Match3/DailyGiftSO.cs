using Match3;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "Daily Gift SO", menuName = "Game/DailyGiftSO")]
    public class DailyGiftSO : ScriptableObject
    {
        public ShopItemID id;
        public int quantity;
        public Sprite sprite;
    }
}
