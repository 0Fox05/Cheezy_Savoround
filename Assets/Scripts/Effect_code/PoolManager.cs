using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Pools")]
    public ObjectPool explosionPool;
    public ObjectPool scoreTextPool;

    [Header("UI")]
    public Canvas worldCanvas;

    private void Awake()
    {
        Instance = this;
    }
}