using UnityEngine;
using UnityEngine.UI; // cần để dùng Button

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject menuPanel;
    public GameObject gameOverPanel;

    // 👉 Public fields để kéo thả Button trong Inspector
    public Button playButton;
    public Button restartButton; // ✅ thêm nút restart

    private void Awake()
    {
        Instance = this;

        // Đăng ký sự kiện click cho button Play
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        // Đăng ký sự kiện click cho button Restart
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }

    public void ShowMenu()
    {
        HideAll();
        menuPanel.SetActive(true);
    }

    public void ShowGame()
    {
        HideAll();
        // nếu có panel gameplay thì bật ở đây
    }

    public void ShowGameOver()
    {
        HideAll();
        gameOverPanel.SetActive(true);
    }

    private void HideAll()
    {
        menuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    // 👉 Hàm xử lý khi bấm nút Play
    public void OnPlayButtonClicked()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    // 👉 Hàm xử lý khi bấm nút Restart
    public void OnRestartButtonClicked()
    {
        GameManager.Instance.RestartGame();
    }
}
