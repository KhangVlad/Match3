using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using System.Collections.Generic;
using System;
using Match3;


public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance;

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;


    // Dictionary to map shop IDs to IAP product IDs
    private Dictionary<string, string> shopIdToProductIdMap = new Dictionary<string, string>();

    private Dictionary<string, string> shopIdWithConditionMap = new Dictionary<string, string>();

    // Event for when a purchase is completed
    public event Action<string> OnPurchaseComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        ShopManager.Instance.OnShopDataLoaded += InitializePurchasing;
    }

    private async void InitializePurchasing()
    {
        if (IsInitialized()) return;
        try
        {
            await Unity.Services.Core.UnityServices.InitializeAsync();
            Debug.Log("Unity Gaming Services initialized and signed in.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Unity Gaming Services: {ex.Message}");
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        // Add products from ShopManager's Purchase_Shop
        foreach (ShopItemPack pack in  ShopManager.Instance.ShopData.Purchase_Shop)
        {
            if (pack.PriceType == PriceType.RealMoney)
            {
                ProductType productType = DetermineProductType(pack);
                string productId = pack.ID.ToLower();
                builder.AddProduct(productId, productType);
                shopIdToProductIdMap[pack.ID] = productId;

                Debug.Log($"Added IAP product: {productId}, Type: {productType}");
            }
        }

        foreach (ShopItemPack pack in  ShopManager.Instance.ShopData.Condition_Shop)
        {
            if (pack.PriceType == PriceType.RealMoney)
            {
                ProductType productType = DetermineProductType(pack);
                string productId = pack.ID.ToLower();
                builder.AddProduct(productId, productType);
                shopIdWithConditionMap[pack.ID] = productId;
            }
        }

        UnityPurchasing.Initialize(this, builder);
    }

    private ProductType DetermineProductType(ShopItemPack pack)
    {
        // Check if this is a non-consumable item like "Remove Ads"
        foreach (ShopItemSlot item in pack.Items)
        {
            if (item.ShopItemID == ShopItemID.RemoveAds)
            {
                return ProductType.NonConsumable;
            }
        }

        // Default to consumable for all other items
        return ProductType.Consumable;
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }


    public void BuyProductByShopId(string shopId)
    {
        if (shopIdToProductIdMap.TryGetValue(shopId, out string productId))
        {
            BuyProduct(productId);
            return;
        }

        if (shopIdWithConditionMap.TryGetValue(shopId, out productId))
        {
            BuyProduct(productId);
            return;
        }

        Debug.LogWarning($"BuyProductByShopId: No product ID mapping found for shop ID {shopId}");
    }

    public void BuyProduct(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"Initiating purchase for: {productId}");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogWarning($"BuyProduct: Product not found or not available: {productId}");
            }
        }
        else
        {
            Debug.LogWarning("BuyProduct: Not initialized");
        }
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        Debug.Log($"Purchase successful: {productId}");
        string shopId = null;

        // First check in regular purchase shop
        foreach (var entry in shopIdToProductIdMap)
        {
            if (entry.Value == productId)
            {
                shopId = entry.Key;

                // Find the shop pack
                ShopItemPack pack = ShopManager.Instance.ShopData.Purchase_Shop.Find(p => p.ID == shopId);

                if (pack != null)
                {
                    OnPurchaseComplete?.Invoke(shopId);
                    return PurchaseProcessingResult.Complete;
                }
            }
        }

        // If not found in purchase shop, check condition shop
        foreach (var entry in shopIdWithConditionMap)
        {
            if (entry.Value == productId)
            {
                shopId = entry.Key;

                // Find the shop pack
                ShopItemPack pack =  ShopManager.Instance.ShopData.Condition_Shop.Find(p => p.ID == shopId);

                if (pack != null)
                {
                    OnPurchaseComplete?.Invoke(shopId);
                    return PurchaseProcessingResult.Complete;
                }
            }
        }

        // If we get here, no matching shop ID was found
        Debug.LogWarning($"ProcessPurchase: No shop ID mapping found for product ID {productId}");

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"OnPurchaseFailed: {product.definition.id}, {failureReason}");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP Initialized");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning("IAP Initialization Failed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning($"IAP Initialization Failed: {error} - {message}");
    }
}