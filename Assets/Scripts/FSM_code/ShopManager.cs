using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public ShopUI shopUI;
    private ShopData shopData;
    private int currentIndex = 0;

    private void Awake()
    {
        Instance = this;
        shopData = LoadShopData();
    }

    void Start()
    {
        ShowCategory("skins"); // default
    }

    public void ReloadShop()
    {
        shopData = LoadShopData();
        currentIndex = 0;
        ShowCategory("skins");
    }

    // ✅ Load ShopData.json from Resources
    private ShopData LoadShopData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("ShopData"); // no .json extension
        if (jsonFile != null)
        {
            ShopData data = JsonUtility.FromJson<ShopData>(jsonFile.text);
            Debug.Log("ShopData loaded successfully from Resources.");
            return data;
        }
        else
        {
            Debug.LogError("ShopData.json not found in Resources!");
            return new ShopData(); // fallback empty data
        }
    }

    public void ShowCategory(string category)
    {
        currentIndex = 0;
        ShowItem(category);
    }

    public List<ShopItem> AllShopItems
    {
        get
        {
            var all = new List<ShopItem>();
            all.AddRange(shopData.shop.skins);
            all.AddRange(shopData.shop.coins);
            all.AddRange(shopData.shop.boosts);
            return all;
        }
    }

    public void NextItem(string category)
    {
        var items = GetList(category);
        if (items.Count == 0) return;
        currentIndex = (currentIndex + 1) % items.Count;
        ShowItem(category);
    }

    public void PreviousItem(string category)
    {
        var items = GetList(category);
        if (items.Count == 0) return;
        currentIndex = (currentIndex - 1 + items.Count) % items.Count;
        ShowItem(category);
    }

    private void ShowItem(string category)
    {
        var items = GetList(category);
        ShopItem item = items.Count > 0 ? items[currentIndex] : null;
        shopUI.ShowItem(item);
    }

    private List<ShopItem> GetList(string category)
    {
        switch (category)
        {
            case "skins": return shopData.shop.skins;
            case "coins": return shopData.shop.coins;
            case "boosts": return shopData.shop.boosts;
            default: return new List<ShopItem>();
        }
    }

    public void AddBooster(string boosterName, int count)
    {
        var booster = GameManager.Instance.playerData.boosters
            .Find(b => b.boosterName == boosterName);

        if (booster != null)
        {
            booster.count += count;   // ✅ increment existing booster
        }
        else
        {
            GameManager.Instance.playerData.boosters.Add(
                new BoosterEntry { id = Guid.NewGuid().ToString(), boosterName = boosterName, count = count });
        }

        SaveSystem.Save(GameManager.Instance.playerData);
    }

    public bool UseBooster(string boosterName)
    {
        PlayerData data = SaveSystem.Load();

        BoosterEntry entry = data.boosters.Find(b => b.boosterName == boosterName);
        if (entry != null && entry.count > 0)
        {
            entry.count--;
            SaveSystem.Save(data);
            Debug.Log($"Used {boosterName}. Remaining: {entry.count}");
            return true;
        }
        Debug.Log($"No {boosterName} boosters left!");
        return false;
    }

    public int GetBoosterCount(string boosterName)
    {
        var booster = GameManager.Instance.playerData.boosters
            .Find(b => b.boosterName == boosterName);

        return booster != null ? booster.count : 0;
    }
}
