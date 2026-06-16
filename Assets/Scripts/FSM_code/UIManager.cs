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
    public GameObject dailyPanel;
    public GameObject achievementPanel;   // ✅ new achievement panel

    [Header("Buttons")]
    public Button playButton;
    public Button restartButton;
    public Button resetButton;
    public Button shopButton;
    public Button dailyButton;
    public Button achievementButton;      // ✅ new achievement button
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
            shopButton.onClick.AddListener(() => TogglePanel(shopPanel));

        if (dailyButton != null)
            dailyButton.onClick.AddListener(() => TogglePanel(dailyPanel));

        if (achievementButton != null)
            achievementButton.onClick.AddListener(() => TogglePanel(achievementPanel));

        if (claimButton != null)
            claimButton.onClick.AddListener(OnClaimClicked);

        RefreshDailyRewardsUI();

        // Panels start hidden
        if (dailyPanel != null) dailyPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (achievementPanel != null) achievementPanel.SetActive(false);

        foreach (Button btn in exitButtons)
        {
            if (btn != null)
                btn.onClick.AddListener(ExitCurrentPanel);
        }

        playerData = SaveSystem.Load();
    }
    public void start()
    {
        HideAll();
        menuPanel.SetActive(true);
    }
    // ✅ Generic toggle for any panel
    public void TogglePanel(GameObject panel)
    {
        bool isActive = panel.activeSelf;
        StopAllCoroutines();
        StartCoroutine(ScalePanel(panel, !isActive));
    }

    public void ShowMenu()
    {
        HideAll();
        menuPanel.SetActive(true);
    }

    public void ShowGame()
    {
        HideAll();
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
        var newData = SaveSystem.Reset();

        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
            shopManager.ReloadShop();

        ShopUI shopUI = FindObjectOfType<ShopUI>();
        if (shopUI != null)
            shopUI.RefreshUI();

        RefreshDailyRewardsUI();
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

        if (playerData.dailyRewardDay >= dailyData.Days.Count)
            playerData.dailyRewardDay = 0;

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

        if (reward.gold > 0)
            playerData.gold += reward.gold;

        if (!string.IsNullOrEmpty(reward.boosterName))
            ShopManager.Instance.AddBooster(reward.boosterName, 1);

        SaveSystem.Save(playerData);
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
                rewardImages[i].color = new Color(1f, 1f, 1f, 0.5f);
                if (rewardTexts != null && i < rewardTexts.Length)
                    rewardTexts[i].text = "Claimed";
            }
            else
            {
                rewardImages[i].color = Color.white;
                if (rewardTexts != null && i < rewardTexts.Length)
                    rewardTexts[i].text = "";
            }
        }
    }

    private void HideAll()
    {
        shopPanel.SetActive(false);
        dailyPanel.SetActive(false);
        achievementPanel.SetActive(false);
        menuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ExitCurrentPanel()
    {
        if (shopPanel.activeSelf)
            TogglePanel(shopPanel);
        else if (dailyPanel.activeSelf)
            TogglePanel(dailyPanel);
        else if (achievementPanel.activeSelf)
            TogglePanel(achievementPanel);
        else if (gameOverPanel.activeSelf)
            gameOverPanel.SetActive(false); // keep slide logic if you want
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
            scoreText.text = score.ToString();
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

    private IEnumerator ScalePanel(GameObject panel, bool show)
    {
        if (show)
            panel.SetActive(true);

        RectTransform rect = panel.GetComponent<RectTransform>();
        Vector3 startScale = show ? Vector3.zero : Vector3.one;
        Vector3 endScale = show ? Vector3.one : Vector3.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        rect.localScale = startScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rect.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localScale = endScale;

        if (!show)
            panel.SetActive(false);
    }
}
