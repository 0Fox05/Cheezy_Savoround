using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementHolder : MonoBehaviour
{
    public int achievementId; // unique ID
    public int targetValue = 100;

    public Image progressBar;
    public TextMeshProUGUI percentageText;

    private int currentValue;

    public void SetProgress(int newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0, targetValue);
        UpdateProgressUI();
    }

    public void UpdateProgressUI()
    {
        float progress = (float)currentValue / targetValue;

        if (progressBar != null)
            progressBar.fillAmount = progress;

        if (percentageText != null)
            percentageText.text = currentValue == 0 ? "" : $"{currentValue}/{targetValue}";
    }
}
