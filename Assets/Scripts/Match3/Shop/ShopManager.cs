using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Match3
{
//     public class ShopManager : MonoBehaviour
//     {
//         public static ShopManager Instance { get; private set; }
//         public ShopData ShopData;
//
//         private void Awake()
//         {
//             if (Instance != null && Instance != this)
//             {
//                 Destroy(this.gameObject);
//                 return;
//             }
//             Instance = this;
//             LoadShopData();
//         }
//
//         private void LoadShopData()
//         {
//             TextAsset shopData = Resources.Load<TextAsset>("Shop/ShopData");
//             if (shopData != null)
//             {
//                 string json = shopData.text;
//                 ShopData = JsonConvert.DeserializeObject<ShopData>(json);
//             }
//         }
//
//         private void Start()
//         {
//             InitializePurchasing();
//         }
//         
//         private void   InitializePurchasing(){}
//     }
//

    [System.Serializable]
    public class ShopData
    {
        public List<ShopItemPack> Purchase_Shop;
        public List<ShopItemPack> Gold_Shop;
        public List<ShopItemPack> Condition_Shop;
    }


    [System.Serializable]
    public class ShopItemPack
    {
        public string ID;
        public string PackName;
        public List<ShopItemSlot> Items;
        public float Price;
        public PriceType PriceType;
        public ShopPackType ShopPackType;
    }


    [System.Serializable]
    public class ShopItemSlot
    {
        public ShopItemID ShopItemID;
        public int Quantity;
    }


    public enum ShopPackType
    {
        Single=0,
        Pair =1,
        Triple=2,
    }
}