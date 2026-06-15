using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Pools")]
    public ObjectPool explosionPool;
    public ObjectPool scoreTextPool;

    // ✅ Add new pools for combo and streak texts
    public ObjectPool comboTextPool;
    public ObjectPool streakTextPool;

    [Header("UI")]
    public Canvas worldCanvas;

    private void Awake()
    {
        Instance = this;
    }
}
