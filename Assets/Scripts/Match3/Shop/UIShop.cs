using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3
{
    public class UIShop : MonoBehaviour
    {
        private Canvas _canvas;
        [SerializeField] private Button _closeBtn;

        [Header("Shop Pack")]
        [SerializeField] private Transform _contentParent;
        [SerializeField] private UIShopPack[] _uiShopPackPrefab;
        [SerializeField] private UIShopPack[] _uiPackSlots;


        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            _closeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                DisplayCanvas(false);
            });
            LoadShopPacks();
        }

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }


        private void LoadShopPacks()
        {
            _uiPackSlots = new UIShopPack[ShopManager.Instance.ShopData.ShopItemPacks.Count];
            for (int i = 0; i < ShopManager.Instance.ShopData.ShopItemPacks.Count; i++)
            {
                ShopItemPack shopPack = ShopManager.Instance.ShopData.ShopItemPacks[i];

                UIShopPack prefab = _uiShopPackPrefab[0];

                if(shopPack.Items.Count == 1)
                {
                    if (shopPack.Items[0].ShopItemID== ShopItemID.Gems)
                    {
                        prefab = _uiShopPackPrefab[1];
                    }
                    else if (shopPack.Items[0].ShopItemID == ShopItemID.RemoveAds)
                    {
                        prefab = _uiShopPackPrefab[2];
                    }
                }
                else if(shopPack.Items.Count == 2)
                {
                    prefab = _uiShopPackPrefab[1];
                }

                UIShopPack uiPackSlot = Instantiate(prefab, _contentParent);
                uiPackSlot.SetPackData(shopPack);

                _uiPackSlots[i] = uiPackSlot;
            }
        }
    }
}

