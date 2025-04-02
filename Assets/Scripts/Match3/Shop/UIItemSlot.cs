using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3
{
    public class UIItemSlot : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _quantityText;


        public void SetItemData(ShopItemSlot itemSlot)
        {
            ShopItemDataSO shopItemData = GameDataManager.Instance.GetShopItemDataByID(itemSlot.ShopItemID);
            _iconImage.sprite = shopItemData.Icon;

            _quantityText.text = $"x{itemSlot.Quantity}";
        }
    }
}

