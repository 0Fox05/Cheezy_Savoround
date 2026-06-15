using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

    [Header("UI References")]
    public Image itemImage;
    public Button buyButton;
    public Button selectButton;
    public Button nextButton;
    public Button previousButton;
    public RectTransform indicator;          // assign your indicator image
    public List<RectTransform> itemSlots;    // assign slots 1,2,3,4 in Inspector
    private int currentIndex = 0;

    [Header("Category Buttons")]
    public Button skinButton;
    public Button coinButton;
    public Button boostButton;

    [Header("Category Panels")]
    public GameObject skinsPanel;   // ✅ new
    public GameObject coinsPanel;   // ✅ new
    public GameObject boostsPanel;  // ✅ new
    [Header("Boost UI")]
    public TextMeshProUGUI boostCountText;   // ✅ assign in Inspector
    public GameObject boostsframe;

    [Header("Player Data")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI costText;

    private ShopItem currentItem;
    private string currentCategory = "skins";

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        buyButton.onClick.AddListener(OnBuyClicked);
        selectButton.onClick.AddListener(OnSelectClicked);

        nextButton.onClick.AddListener(() =>
        {
            FindObjectOfType<ShopManager>().NextItem(currentCategory);
            MoveIndicator(currentIndex + 1); // move indicator forward
        });

        previousButton.onClick.AddListener(() =>
        {
            FindObjectOfType<ShopManager>().PreviousItem(currentCategory);
            MoveIndicator(currentIndex - 1); // move indicator backward
        });

        skinButton.onClick.AddListener(() => SwitchCategory("skins"));
        coinButton.onClick.AddListener(() => SwitchCategory("coins"));
        boostButton.onClick.AddListener(() => SwitchCategory("boosts"));

        UpdateGoldUI();
        ShowCategoryPanel(currentCategory);
        ResetIndicator();
        RefreshUI();
    }

    public void ResetIndicator()
    {
        MoveIndicator(0); // back to slot 1
    }

    public void ShowItem(ShopItem item)
    {
        currentItem = item;

        if (item == null)
        {
            itemImage.sprite = null;
            buyButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(false);
            costText.text = ""; // clear cost
            return;
        }

        itemImage.sprite = Resources.Load<Sprite>(item.prefabName);

        // ✅ update cost text
        if (costText != null)
        {
            costText.text = $"{item.price}";
        }

        if (currentCategory == "skins")
        {
            buyButton.gameObject.SetActive(!item.owned);
            selectButton.gameObject.SetActive(item.owned);
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            selectButton.gameObject.SetActive(false);
        }
    }

    private void OnBuyClicked()
    {
        if (currentItem == null) return;

        // Check if player has enough gold
        if (GameManager.Instance.playerData.gold >= currentItem.price)
        {
            // Spend gold
            GameManager.Instance.playerData.gold -= currentItem.price;

            // Handle by category
            if (currentCategory == "skins")
            {
                currentItem.owned = true;

                // ✅ Save ownership to PlayerData
                if (!GameManager.Instance.playerData.skins.Contains(currentItem.prefabName))
                {
                    GameManager.Instance.playerData.skins.Add(currentItem.prefabName);
                }

                Debug.Log($"Bought skin {currentItem.name} for {currentItem.price} gold.");
                SaveSystem.Save(GameManager.Instance.playerData);
            }
            else if (currentCategory == "coins")
            {
                // Coins: add amount to player gold
                GameManager.Instance.playerData.gold += currentItem.addAmount;
                Debug.Log($"Bought coin pack {currentItem.name}, added {currentItem.addAmount} gold.");
            }
            else if (currentCategory == "boosts")
            {
                ShopManager.Instance.AddBooster(currentItem.name, 1);
                Debug.Log($"Bought booster {currentItem.name}, +1 count.");

                // ✅ refresh booster count text
                if (boostCountText != null)
                {
                    int boosterCount = ShopManager.Instance.GetBoosterCount(currentItem.name);
                    boostCountText.text = $"x{boosterCount}";
                }
            }
            // Refresh UI
            ShowItem(currentItem);
            UpdateGoldUI();

            // Save updated player data
            SaveSystem.Save(GameManager.Instance.playerData);
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }


    private void OnSelectClicked()
    {
        if (currentItem == null) return;

        Debug.Log($"Selected skin: {currentItem.name}");

        PlayerData data = SaveSystem.Load();
        data.equippedSkinPrefab = currentItem.prefabName;
        SaveSystem.Save(data);
        PlateSpawner.Instance.RefreshSkin();
    }

    private void SwitchCategory(string category)
    {
        currentCategory = category;
        FindObjectOfType<ShopManager>().ShowCategory(category);
        ShowCategoryPanel(category); // ✅ show/hide panels
    }

    private void ShowCategoryPanel(string category)
    {
        skinsPanel.SetActive(category == "skins");
        coinsPanel.SetActive(category == "coins");
        boostsPanel.SetActive(category == "boosts");

        boostsframe.SetActive(boostsPanel.activeSelf);

        if (category == "skins")
        {
            // ✅ Sync ownership from PlayerData
            foreach (var item in ShopManager.Instance.AllShopItems)
            {
                item.owned = GameManager.Instance.playerData.skins.Contains(item.prefabName);
            }
        }

        if (boostsPanel.activeSelf && currentItem != null)
        {
            int boosterCount = ShopManager.Instance.GetBoosterCount(currentItem.name);

            if (boostCountText != null)
            {
                boostCountText.text = $"x{boosterCount}";
            }
        }
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = $"{GameManager.Instance.playerData.gold}";
        }
    }
    public void RefreshUI()
    {
        // Update gold text
        UpdateGoldUI();

        // Refresh current item display
        if (currentItem != null)
        {
            ShowItem(currentItem);
        }
        else
        {
            // If no item selected, show first item in category
            FindObjectOfType<ShopManager>().ShowCategory(currentCategory);
        }

        Debug.Log("Shop UI refreshed.");
    }
    public void MoveIndicator(int newIndex)
    {
        if (indicator == null || itemSlots.Count == 0) return;

        // Wrap index around instead of clamping
        if (newIndex < 0)
            newIndex = itemSlots.Count - 1; // go to last slot
        else if (newIndex >= itemSlots.Count)
            newIndex = 0; // loop back to first slot

        currentIndex = newIndex;

        // Teleport indicator to the slot position
        indicator.position = itemSlots[currentIndex].position;
    }
}
