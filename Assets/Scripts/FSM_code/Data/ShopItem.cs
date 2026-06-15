using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string id;
    public string name;
    public int price;
    public string prefabName;

    // ✅ For skins
    public bool owned;

    // ✅ For coins
    public int addAmount;
}

[System.Serializable]
public class ShopCategory
{
    public List<ShopItem> skins = new List<ShopItem>();
    public List<ShopItem> boosts = new List<ShopItem>();
    public List<ShopItem> coins = new List<ShopItem>();
}

[System.Serializable]
public class ShopData
{
    public ShopCategory shop = new ShopCategory();
}
