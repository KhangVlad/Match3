using UnityEngine;
using RoboRyanTron.SearchableEnum;

namespace Match3
{
    [CreateAssetMenu(fileName = "Shop Item", menuName = "ChillHavenStory/Shop Item")]
    public class ShopItemDataSO : ScriptableObject
    {
        [SearchableEnum]
        public ShopItemID ShopItemID;
        public Sprite Icon;
    }

}
