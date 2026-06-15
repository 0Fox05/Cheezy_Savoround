using UnityEngine;

public class ComboStreakSystem : MonoBehaviour
{
    public static ComboStreakSystem instance;

    private int currentCombo = 0;     // combo count within this cycle
    private int currentStreak = 0;    // streak across cycles
    private bool clearedThisCycle = false;

    private void Awake() => instance = this;

    // ✅ Call at the start of each cycle
    public void StartCycle()
    {
        currentCombo = 0;
        clearedThisCycle = false;
        Debug.Log("Cycle started");
    }

    // ✅ Called whenever a plate is cleared during the cycle
    public void RegisterPlateClear(Plate plate)
    {
        if (plate.IsFull() && plate.IsPure())
        {
            currentCombo++;
            clearedThisCycle = true;
            Debug.Log("Registered clear → Combo: " + currentCombo);
        }
    }

    // ✅ Call once at the end of the cycle
    public void EndCycle()
    {
        // Show combo text if 2+ plates cleared in this cycle
        if (currentCombo >= 2)
        {
            var comboObj = PoolManager.Instance.comboTextPool.Get();
            comboObj.transform.SetParent(PoolManager.Instance.worldCanvas.transform, false);
            comboObj.transform.position = Vector3.zero + Vector3.up * 2f;
            comboObj.GetComponent<FloatingText>().Show("Combo " + currentCombo);
        }

        // Handle streak logic
        if (clearedThisCycle)
        {
            currentStreak++; // ✅ streak grows only if something cleared
            if (currentStreak - 1 >= 1)
            {
                var streakObj = PoolManager.Instance.streakTextPool.Get();
                streakObj.transform.SetParent(PoolManager.Instance.worldCanvas.transform, false);
                streakObj.transform.position = Vector3.zero + Vector3.up * 3f;
                streakObj.GetComponent<FloatingText>().Show("Streak " + (currentStreak - 1));
            }
        }
        else
        {
            currentStreak = 0; // ✅ reset immediately if no clears
        }

        Debug.Log($"Cycle ended → Combo: {currentCombo}, Streak: {currentStreak}");
    }
}
