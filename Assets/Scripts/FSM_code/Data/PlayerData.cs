using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoosterEntry
{
    public string id;
    public string boosterName;
    public int count;
}
[System.Serializable]
public class AchievementEntry
{
    public int id;          // unique achievement ID
    public int progress;    // current progress
    public bool completed;  // optional flag
}

[System.Serializable]
public class PlayerData
{
    public int gold = 100;
    public List<string> skins = new List<string>();
    public List<string> missions = new List<string>();
    public List<string> achievements = new List<string>();

    // ✅ Achievements tracked by ID
    public List<AchievementEntry> achievementProgress = new List<AchievementEntry>();

    public string equippedSkinPrefab = "";

    // Daily rewards
    public string lastDailyClaimUTC = "";
    public int dailyRewardDay = 0;
    public List<bool> dailyRewardsClaimed = new List<bool>(new bool[7]);

    // Boosters
    public List<BoosterEntry> boosters = new List<BoosterEntry>();
}