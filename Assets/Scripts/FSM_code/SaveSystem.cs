using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string fileName = "PlayerData.json";
    private static string filePath = Path.Combine(Application.persistentDataPath, fileName);

    // Save data to JSON (persistent path)
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
            // ✅ Load from persistent path if save exists
            string json = File.ReadAllText(filePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("Data loaded from: " + filePath);
            return data;
        }
        else
        {
            // ✅ First run: load default from Resources
            TextAsset jsonFile = Resources.Load<TextAsset>("PlayerData");
            if (jsonFile != null)
            {
                PlayerData data = JsonUtility.FromJson<PlayerData>(jsonFile.text);
                Save(data); // write to persistent path for future use
                Debug.Log("Default PlayerData loaded from Resources.");
                return data;
            }
            else
            {
                Debug.LogWarning("PlayerData.json not found in Resources, creating empty data.");
                PlayerData newData = new PlayerData();
                Save(newData);
                return newData;
            }
        }
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
