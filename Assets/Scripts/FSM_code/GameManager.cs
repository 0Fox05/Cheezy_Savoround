using UnityEngine;
using UnityEngine.SceneManagement; // ✅ needed for scene reload

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState CurrentState;
    private int score = 0;

    private void Start()
    {
        ChangeState(GameState.Menu);
    }

    private void Awake()
    {
        Instance = this;
    }

    public void AddScore(int point)
    {
        score += point;
        Debug.Log($"Score increased by {point}, total: {score}");
    }

    public int GetScore()
    {
        return score;
    }

    public void ResetScore()
    {
        score = 0;
        Debug.Log("Score reset to 0");
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("Current State: " + newState);

        switch (newState)
        {
            case GameState.Menu:
                UIManager.Instance.ShowMenu();
                break;
            case GameState.Playing:
                UIManager.Instance.ShowGame();
                PlateSpawner.Instance.CheckSpawnPoints();
                break;
            case GameState.Sorting:
                break;
            case GameState.GameOver:
                UIManager.Instance.ShowGameOver();
                break;
        }
    }

    // ✅ Restart the whole game by reloading the current scene
    public void RestartGame()
    {
        ResetScore(); // optional: reset score before restart
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
