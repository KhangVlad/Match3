#if !UNITY_WEBGL
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Match3;

public class UIShopPack : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _packageName;
    [SerializeField] private Image mainImage;
    [SerializeField] private TextMeshProUGUI _priceText;

    [SerializeField] private Button _buyBtn;

    [Header("Contents")] [SerializeField] private Transform _contentParent;
    [SerializeField] private UIItemSlot[] _uiItemSlotPrefab;
    [SerializeField] private UIItemSlot[] _uiItemSlots;

    private ShopItemPack _cachedShopItemPackage;

    // Event for when a purchase is completed
    public event Action<string> OnPurchaseComplete;

    private void Start()
    {
        _buyBtn.onClick.AddListener(OnBuyButtonClicked);

        // Subscribe to IAP purchase events if this is a real money purchase
        if (_cachedShopItemPackage != null && _cachedShopItemPackage.PriceType == PriceType.RealMoney &&
            IAPManager.Instance != null)
        {
            IAPManager.Instance.OnPurchaseComplete += HandleIAPPurchaseComplete;
        }
    }

    private void OnDestroy()
    {
        _buyBtn.onClick.RemoveAllListeners();

        // Unsubscribe from IAP events
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnPurchaseComplete -= HandleIAPPurchaseComplete;
        }
    }

    private void HandleIAPPurchaseComplete(string shopId)
    {
        if (_cachedShopItemPackage != null && _cachedShopItemPackage.ID == shopId)
        {
            ProcessPurchaseItems();
            OnPurchaseComplete?.Invoke(shopId);
        }

        if (!UserManager.Instance.UserData.IsBuyWelcomePack && shopId == "welcome")
        {
            UserManager.Instance.UserData.IsBuyWelcomePack = true;
        }
    }

    private void OnBuyButtonClicked()
    {
        AudioManager.Instance.PlayButtonSfx();

        if (_cachedShopItemPackage == null) return;

        // Check if this is a real money purchase
        if (_cachedShopItemPackage.PriceType == PriceType.RealMoney)
        {
            // For real money purchases, use IAP
            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.BuyProductByShopId(_cachedShopItemPackage.ID);
            }
            else
            {
                Debug.LogError("IAPManager instance not found!");
            }
        }
        else if (_cachedShopItemPackage.PriceType == PriceType.Gold)
        {
            if (UserManager.Instance.HasEnoughGold(_cachedShopItemPackage.Price))
            {
                UserManager.Instance.ConsumeGold(_cachedShopItemPackage.Price);
                ProcessPurchaseItems();
                OnPurchaseComplete?.Invoke(_cachedShopItemPackage.ID);
            }
            else
            {
                Debug.Log("Not enough gold!");
                // Show "Not enough gold" message
            }
        }
    }

    private void ProcessPurchaseItems()
    {
        for (int i = 0; i < _cachedShopItemPackage.Items.Count; i++)
        {
            ShopItemSlot itemSlot = _cachedShopItemPackage.Items[i];
            switch (itemSlot.ShopItemID)
            {
                case ShopItemID.Gold:
                    UserManager.Instance.AddGold(itemSlot.Quantity);
                    break;
                case ShopItemID.Energy:
                    UserManager.Instance.RestoreEnergy(itemSlot.Quantity);
                    break;
                case ShopItemID.RemoveAds:
                    Debug.Log("Ads Removed!");
                    break;
                case ShopItemID.ColorBurst:
                    GameplayUserManager.Instance.AddBooster(BoosterID.ColorBurst, itemSlot.Quantity);
                    break;

                case ShopItemID.BlastBomb:
                    GameplayUserManager.Instance.AddBooster(BoosterID.BlastBomb, itemSlot.Quantity);
                    break;

                case ShopItemID.AxisBomb:
                    GameplayUserManager.Instance.AddBooster(BoosterID.AxisBomb, itemSlot.Quantity);
                    break;

                case ShopItemID.ExtraMove:
                    GameplayUserManager.Instance.AddBooster(BoosterID.ExtraMove, itemSlot.Quantity);
                    break;

                case ShopItemID.FreeSwitch:
                    GameplayUserManager.Instance.AddBooster(BoosterID.FreeSwitch, itemSlot.Quantity);
                    break;

                case ShopItemID.Hammer:
                    GameplayUserManager.Instance.AddBooster(BoosterID.Hammer, itemSlot.Quantity);
                    break;
            }
        }
    }

    public void SetPackData(ShopItemPack packageData, Sprite main = null)
    {
        if (main is not null)
        {
            this.mainImage.sprite = main;
        }

        _cachedShopItemPackage = packageData;

        _packageName.text = packageData.PackName;

        // Format price text based on price type
        if (packageData.PriceType == PriceType.RealMoney)
        {
            _priceText.text = $"${packageData.Price:0.00}";

        }
        else if (packageData.PriceType == PriceType.Gold)
        {
            _priceText.text = $"{packageData.Price:0} Gold";
        }

        // Clear any existing item slots
        foreach (Transform child in _contentParent)
        {
            Destroy(child.gameObject);
        }

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
#endif