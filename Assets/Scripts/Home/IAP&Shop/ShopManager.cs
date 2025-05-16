using System;
using UnityEngine;
using System.Collections.Generic;
using Match3;
using Newtonsoft.Json;
using UnityEngine.Serialization;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    // Event for when shop data is loaded
    public event Action OnShopDataLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public ShopData ShopData;

    private  void Start()
    {
         LoadShopData();
    }

    private  void LoadShopData()
    {
        TextAsset shopData = Resources.Load<TextAsset>("Shop/ShopData");
        if (shopData != null)
        {
            string json = shopData.text;
            this.ShopData = JsonConvert.DeserializeObject<ShopData>(json);
            OnShopDataLoaded?.Invoke();
        }
        else
        {
            Debug.LogError("Failed to load shop data!");
        }
    }

    // Helper method to get a shop pack by ID
    public ShopItemPack GetShopPackById(string id)
    {
        // Check Purchase_Shop
        ShopItemPack pack = ShopData.Purchase_Shop.Find(p => p.ID == id);
        if (pack != null) return pack;

        // Check Gold_Shop
        ShopItemPack pack_gold = ShopData.Gold_Shop.Find(p => p.ID == id);
        if (pack_gold != null) return pack_gold;

        return ShopData.Condition_Shop.Find(p => p.ID == id);
    }
}