using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState CurrentState;

    public PlayerData playerData; // ✅ add this line

    private int score = 0;
    private int scoreBest = 0;

    private void Awake()
    {
        Instance = this;
        playerData = SaveSystem.Load(); // ✅ load player data when game starts
    }
    public int GetScore()
    {
        return score;
    }
    public int GetBestScore()
    {
        return scoreBest;
    }
    public void ResetScore()
    {
        score = 0;
    }
    private void Start()
    {
        PlayerData data = SaveSystem.Load();
        if (!string.IsNullOrEmpty(data.equippedSkinPrefab))
        {
            GameObject skinPrefab = Resources.Load<GameObject>(data.equippedSkinPrefab);
            if (skinPrefab != null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                        // Remove old skin if present
                        Transform oldSkin = player.transform.Find("Skin");
                        if (oldSkin != null) Destroy(oldSkin.gameObject);

                        // Instantiate new skin prefab
                        GameObject newSkin = Instantiate(skinPrefab, player.transform);
                        newSkin.name = "Skin";
                }
            }
        }
        ChangeState(GameState.Menu);
        RefreshGameState();
    }

    public void AddScore(int point)
    {
        score += point;
        if (scoreBest < score)
            scoreBest = score;
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Menu:
                UIManager.Instance.ShowMenu();
                break;
            case GameState.Playing:
                UIManager.Instance.ShowGame();
                PlateSpawner.Instance.CheckSpawnPoints();
                break;
            case GameState.GameOver:
                AwardGold(score); // ✅ give gold at game over
                UIManager.Instance.ShowGameOver();
                break;
        }
    }

    public void AwardGold(int score)
    {
        int goldEarned = score / 10;
        if (goldEarned > 0)
        {
            int currentGold = SaveSystem.GetGold();
            SaveSystem.SetGold(currentGold + goldEarned);
            Debug.Log($"Game Over: earned {goldEarned} gold. Total gold: {SaveSystem.GetGold()}");
        }
    }

    public void IncreaseAchievementProgress(int id, int amount = 1)
    {

        PlayerData data = SaveSystem.Load();

        // Find entry by ID
        AchievementEntry entry = data.achievementProgress.Find(a => a.id == id);
        if (entry == null)
        {
            entry = new AchievementEntry { id = id, progress = 0, completed = false };
            data.achievementProgress.Add(entry);
        }

        entry.progress += amount;

        // Clamp to target if you want
        AchievementHolder holder = AchievementManager.Instance.holders.Find(h => h.achievementId == id);
        if (holder != null)
        {
            if (entry.progress >= holder.targetValue)
            {
                entry.progress = holder.targetValue;
                entry.completed = true;
            }
            holder.SetProgress(entry.progress);
        }

        SaveSystem.Save(data);
        Debug.Log("Game Achievement up");
    }

    public void RefreshGameState()
    {
        // refresh UI
        UIManager.Instance.UpdateScore(score);
        ShopUI.Instance.RefreshUI();

        // Refresh plate skin
        PlateSpawner.Instance.RefreshSkin();


        Debug.Log("Game state refreshed.");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        playerData = SaveSystem.Load();
        RefreshGameState();
        ResetScore();
    }
}
