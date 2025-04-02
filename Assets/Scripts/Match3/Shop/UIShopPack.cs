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


        private void Start()
        {
            _buyBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();


            });
        }

        private void OnDestroy()
        {
            _buyBtn.onClick.RemoveAllListeners();
        }


        public void SetPackData(ShopItemPack packData)
        {
            _packageName.text = packData.PackName;
            _priceText.text = $"{packData.Price}$";


            _uiItemSlots = new UIItemSlot[packData.Items.Count];
            for (int i = 0; i < packData.Items.Count; i++)
            {
                ShopItemSlot shopItemSlot = packData.Items[i];

                UIItemSlot prefab = _uiItemSlotPrefab[0];

                if (packData.Items[i].ShopItemID == ShopItemID.RemoveAds)
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

