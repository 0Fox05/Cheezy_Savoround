using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public PlayerData playerData;

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject gameOverPanel;
    public GameObject shopPanel;
    public GameObject dailyPanel;   // ✅ new daily panel

    [Header("Buttons")]
    public Button playButton;
    public Button restartButton;
    public Button resetButton;
    public Button shopButton;
    public Button dailyButton;      // ✅ new daily button
    public List<Button> exitButtons;

    [Header("Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreBestText;

    private RectTransform gameOverRect;

    [Header("Daily stuff")]
    public Button claimButton;
    public Image[] rewardImages;
    public TextMeshProUGUI[] rewardTexts;
    private DailyData dailyData;

    private void Awake()
    {
        Instance = this;

        string path = Path.Combine(Application.streamingAssetsPath, "DailyData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            dailyData = JsonUtility.FromJson<DailyData>(json);
            Debug.Log("DailyData loaded successfully from StreamingAssets.");
        }
        else
        {
            Debug.LogError($"DailyData.json not found at path: {path}");
        }

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (gameOverPanel != null)
            gameOverRect = gameOverPanel.GetComponent<RectTransform>();

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetAllData);

        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShop);

        if (dailyButton != null)
            dailyButton.onClick.AddListener(ToggleDailyPanel);

        claimButton.onClick.AddListener(OnClaimClicked);

        RefreshDailyRewardsUI();

        if (dailyPanel != null)
            dailyPanel.SetActive(false);

        foreach (Button btn in exitButtons)
        {
            if (btn != null)
                btn.onClick.AddListener(ExitCurrentPanel);
        }
        playerData = SaveSystem.Load();
    }

    // ✅ Toggle daily panel on/off
    public void ToggleDailyPanel()
    {
        bool isActive = dailyPanel.activeSelf;
        dailyPanel.SetActive(!isActive);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        GameManager.Instance.RefreshGameState();
    }

    public void ShowMenu()
    {
        HideAll();
        menuPanel.SetActive(true);
    }

    public void ShowGame()
    {
        HideAll();
        // If you have a gameplay panel, enable it here
    }

    public void ShowGameOver()
    {
        HideAll();
        gameOverPanel.SetActive(true);

        if (scoreText != null)
            scoreText.text = GameManager.Instance.GetScore().ToString();
        if (scoreBestText != null)
            scoreBestText.text = GameManager.Instance.GetBestScore().ToString();

        if (gameOverRect != null)
            StartCoroutine(SlideDown(gameOverRect));
    }

    private void ResetAllData()
    {
        // Reset file
        var newData = SaveSystem.Reset();

        // ✅ Reload shop data
        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            shopManager.ReloadShop();  // custom method to reload JSON and reset UI
        }

        // ✅ Refresh Shop UI
        ShopUI shopUI = FindObjectOfType<ShopUI>();
        if (shopUI != null)
        {
            shopUI.RefreshUI();
        }

        // ✅ Refresh Daily Rewards UI
        RefreshDailyRewardsUI();

        // ✅ Refresh score UI
        UpdateScore(0);

        GameManager.Instance.RestartGame();

        Debug.Log("All data reset and UIs refreshed.");
    }

    public void OnClaimClicked()
    {
        playerData = SaveSystem.Load();

        if (!SaveSystem.CanClaimDailyReward())
        {
            Debug.Log("Reward already claimed today.");
            return;
        }

        // Loop rewards weekly
        if (playerData.dailyRewardDay >= dailyData.Days.Count)
        {
            playerData.dailyRewardDay = 0; // reset to first day
        }

        DayReward reward = dailyData.Days[playerData.dailyRewardDay];
        ClaimDailyReward(reward);

        playerData.dailyRewardDay++;
        playerData.lastDailyClaimUTC = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        SaveSystem.Save(playerData);

        RefreshDailyRewardsUI();
    }

    public void ClaimDailyReward(DayReward reward)
    {
        playerData = SaveSystem.Load();

        // Apply gold reward
        if (reward.gold > 0)
        {
            playerData.gold = playerData.gold + reward.gold;
        }

        // Apply booster reward
        if (!string.IsNullOrEmpty(reward.boosterName))
        {
            ShopManager.Instance.AddBooster(reward.boosterName, 1);
        }

        // Save back to JSON
        SaveSystem.Save(playerData);

        // Sync GameManager
        GameManager.Instance.playerData = playerData;

        Debug.Log($"Daily reward claimed. Gold after claim: {playerData.gold}");
        GameManager.Instance.RefreshGameState();
    }

    public void RefreshDailyRewardsUI()
    {
        PlayerData data = SaveSystem.Load();

        for (int i = 0; i < rewardImages.Length; i++)
        {
            if (i < data.dailyRewardDay)
            {
                // Mark as claimed
                rewardImages[i].color = new Color(1f, 1f, 1f, 0.5f); // faded
                if (rewardTexts != null && i < rewardTexts.Length)
                {
                    rewardTexts[i].text = "Claimed";   // ✅ show text
                }
            }
            else
            {
                // Not yet claimed
                rewardImages[i].color = Color.white;
                if (rewardTexts != null && i < rewardTexts.Length)
                {
                    rewardTexts[i].text = "";          // clear text
                }
            }
        }
    }


    private void HideAll()
    {
        shopPanel.SetActive(false);
        menuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ExitCurrentPanel()
    {
        if (shopPanel.activeSelf)
        {
            shopPanel.SetActive(false);
        }
        if (gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(false);
        }
        else if (menuPanel.activeSelf)
        {
            Debug.Log("Menu panel is active — Exit has no effect.");
        }
    }

    public void OnPlayButtonClicked()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "" + score.ToString();
    }

    private IEnumerator SlideDown(RectTransform panel)
    {
        Vector2 startPos = new Vector2(0, Screen.height);
        Vector2 endPos = Vector2.zero;

        panel.anchoredPosition = startPos;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            panel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = endPos;
    }
}
