using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string fileName = "PlayerData.json";
    private static string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

    // Save data to JSON
    public static void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Data saved to: " + filePath);
        GameManager.Instance.RefreshGameState();
    }

    // Load data from JSON
    public static PlayerData Load()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("Data loaded from: " + filePath);
            return data;
        }
        else
        {
            Debug.LogWarning("Save file not found, creating new default data.");
            PlayerData newData = new PlayerData();
            Save(newData);
            return newData;
        }
        GameManager.Instance.RefreshGameState();
    }

    // Reset data to defaults
    public static PlayerData Reset()
    {
        PlayerData newData = new PlayerData();

        // default skin
        newData.skins.Add("Skins/Classic");
        // default boosters
        newData.boosters.Add(new BoosterEntry { id = "1", boosterName = "MakePure", count = 0 });
        newData.boosters.Add(new BoosterEntry { id = "2", boosterName = "MakeFull", count = 0 });
        newData.boosters.Add(new BoosterEntry { id = "3", boosterName = "Throw", count = 0 });
        newData.boosters.Add(new BoosterEntry { id = "4", boosterName = "Swap", count = 0 });
        

        Save(newData);
        Debug.Log("Data reset to defaults.");
        return newData;
        GameManager.Instance.RefreshGameState();
    }

    // ✅ Get player's gold directly
    public static int GetGold()
    {
        PlayerData data = Load();
        return data.gold;
    }

    // ✅ Set player's gold directly
    public static void SetGold(int amount)
    {
        PlayerData data = Load();
        data.gold = amount;
        Save(data);
        GameManager.Instance.RefreshGameState();
    }

    public static bool CanClaimDailyReward()
    {
        PlayerData data = Load();
        if (string.IsNullOrEmpty(data.lastDailyClaimUTC))
            return true;

        DateTime lastClaim = DateTime.Parse(data.lastDailyClaimUTC);
        DateTime now = DateTime.UtcNow;

        // ✅ Only allow claim if a new day has started
        return (now.Date > lastClaim.Date);
    }


    public static void ClaimDailyReward()
    {
        PlayerData data = Load();
        string todayUTC = System.DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (data.lastDailyClaimUTC != todayUTC)
        {
            // Example: rewards per day
            int[] rewardGold = { 5, 10, 15, 20, 25, 30, 50 };

            int dayIndex = data.dailyRewardDay % rewardGold.Length;
            data.gold += rewardGold[dayIndex];
            data.lastDailyClaimUTC = todayUTC;
            data.dailyRewardDay++;

            Save(data);
            Debug.Log($"Claimed day {dayIndex + 1} reward: +{rewardGold[dayIndex]} gold. Total: {data.gold}");
        }
        else
        {
            Debug.Log("Already claimed today.");
        }
    }
}
