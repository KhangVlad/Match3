using System;
using UnityEngine;
using System.Collections.Generic;
using Match3;

[CreateAssetMenu(fileName = "Shop cash item", menuName = "Game/Shop Cash")]
public class ShopCashSO : ScriptableObject
{
    public string id;
    public string namePackage;
    public string description;
    public Cost costPrice;
    public PackageType packageType;
    public List<BoosterAndAmount> boosters;
    public float gold;
    public int energy;
}

[Serializable]
public class BoosterAndAmount
{
    public BoosterID BoosterID;
    public int amount;
}

[Serializable]
public class Cost
{
    public PriceType priceType;
    public float price;
}

public enum PackageType
{
    Small = 0,
    Medium = 1,
    Large = 2,
    Mega = 3,
}

public enum PriceType
{
    Gold =0,
    RealMoney=1
}

public enum IAPType : byte
{
    Consumable = 0,     // can purchase again and again
    NonConsumable = 1,  // one-time only
    Subscription = 2,   // specific time period (ex: 1 month ,...)
}