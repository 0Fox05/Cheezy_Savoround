using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoosterReward
{
    public string id;
    public string boosterName;
    public int count;
}

[System.Serializable]
public class DayReward
{
    public int id;
    public int gold;
    public string boosterName; // instead of BoosterEntry
}

[System.Serializable]
public class DailyData
{
    public List<DayReward> Days = new List<DayReward>();
}
