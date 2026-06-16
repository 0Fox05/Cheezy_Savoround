using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public List<AchievementHolder> holders;

    private void Awake()
    {
        Instance = this;
        if (holders == null || holders.Count == 0)
            holders = new List<AchievementHolder>(GetComponentsInChildren<AchievementHolder>());
    }

    private void Start()
    {
        PlayerData data = SaveSystem.Load();

        // Ensure every holder has an entry in PlayerData
        foreach (var holder in holders)
        {
            AchievementEntry entry = data.achievementProgress.Find(a => a.id == holder.achievementId);
            if (entry == null)
            {
                entry = new AchievementEntry { id = holder.achievementId, progress = 0, completed = false };
                data.achievementProgress.Add(entry);
            }
            holder.SetProgress(entry.progress);
        }

        SaveSystem.Save(data);
    }

    // ✅ Increase achievement progress by ID order
    public void IncreaseAchievement(int id, int amount = 1)
    {
        PlayerData data = SaveSystem.Load();

        AchievementEntry entry = data.achievementProgress.Find(a => a.id == id);
        if (entry == null)
        {
            entry = new AchievementEntry { id = id, progress = 0, completed = false };
            data.achievementProgress.Add(entry);
        }

        entry.progress += amount;

        AchievementHolder holder = holders.Find(h => h.achievementId == id);
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
    }
}
