using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public ShopData ShopData;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        LoadShopData();
    }

    private void LoadShopData()
    {
        TextAsset shopData = Resources.Load<TextAsset>("Shop/ShopData");
        Debug.Log(shopData == null);
        if(shopData != null)
        {
            Debug.Log(shopData.text);
            string json = shopData.text;
            ShopData = JsonConvert.DeserializeObject<ShopData>(json);
        }
    }
}

[System.Serializable]
public class ShopData
{
    public List<ShopItemPack> ShopItemPacks;
}



[System.Serializable]
public class ShopItemPack 
{
    public string PackName;
    public List<ShopItemSlot> Items;
    public float Price;
}


[System.Serializable]
public class ShopItemSlot
{
    public ShopItemID ShopItemID;
    public int Quantity;
}
