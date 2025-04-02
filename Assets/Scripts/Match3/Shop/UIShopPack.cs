using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3
{
    public class UIShopPack : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _packageName;
        [SerializeField] private TextMeshProUGUI _priceText;

        [SerializeField] private Button _buyBtn;


        [Header("Contents")]
        [SerializeField] private Transform _contentParent;
        [SerializeField] private UIItemSlot[] _uiItemSlotPrefab;
        [SerializeField] private UIItemSlot[] _uiItemSlots;

        private ShopItemPack _cachedShopItemPackage;


        private void Start()
        {
            _buyBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                for (int i = 0; i < _cachedShopItemPackage.Items.Count; i++)
                {
                    ShopItemSlot itemSlot = _cachedShopItemPackage.Items[i];
                    switch (itemSlot.ShopItemID)
                    {
                        case ShopItemID.Gems:
                            Debug.Log("Add gems");
                            break;
                        case ShopItemID.RemoveAds:
                            Debug.Log("Add remove ads");
                            break;
                        case ShopItemID.ColorBurst:
                            UserManager.Instance.AddBooster(BoosterID.ColorBurst, itemSlot.Quantity);
                            break;
                        case ShopItemID.BlastBomb:
                            UserManager.Instance.AddBooster(BoosterID.BlastBomb, itemSlot.Quantity);
                            break;
                        case ShopItemID.AxisBomb:
                            UserManager.Instance.AddBooster(BoosterID.AxisBomb, itemSlot.Quantity);
                            break;
                        case ShopItemID.ExtraMove:
                            UserManager.Instance.AddBooster(BoosterID.ExtraMove, itemSlot.Quantity);
                            break;
                        case ShopItemID.FreeSwitch:
                            UserManager.Instance.AddBooster(BoosterID.FreeSwitch, itemSlot.Quantity);
                            break;
                        case ShopItemID.Hammer:
                            UserManager.Instance.AddBooster(BoosterID.Hammer, itemSlot.Quantity);
                            break;
                    }
                }
            });
        }

        private void OnDestroy()
        {
            _buyBtn.onClick.RemoveAllListeners();
        }


        public void SetPackData(ShopItemPack packageData)
        {
            _cachedShopItemPackage = packageData;

            _packageName.text = packageData.PackName;
            _priceText.text = $"{packageData.Price}$";


            _uiItemSlots = new UIItemSlot[packageData.Items.Count];
            for (int i = 0; i < packageData.Items.Count; i++)
            {
                ShopItemSlot shopItemSlot = packageData.Items[i];

                UIItemSlot prefab = _uiItemSlotPrefab[0];

                if (packageData.Items[i].ShopItemID == ShopItemID.RemoveAds)
                {
                    prefab = _uiItemSlotPrefab[1];
                }

                UIItemSlot uIItemSlot = Instantiate(prefab, _contentParent);
                uIItemSlot.SetItemData(shopItemSlot);

                _uiItemSlots[i] = uIItemSlot;
            }
        }
    }
}

