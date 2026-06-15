using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlateSpawner : MonoBehaviour
{
    public static PlateSpawner Instance;

    public GameObject defaultPlatePrefab;   // fallback if no skin equipped
    public List<GameObject> pizzaPrefabs;
    public Transform[] spawnPoints;

    private GameObject activePlatePrefab;

    private void Awake()
    {
        Instance = this;

        // Load player data
        PlayerData data = SaveSystem.Load();

        if (!string.IsNullOrEmpty(data.equippedSkinPrefab))
        {
            // Try to load the equipped skin prefab from Resources
            activePlatePrefab = Resources.Load<GameObject>(data.equippedSkinPrefab);
        }

        // Fallback if nothing equipped or prefab not found
        if (activePlatePrefab == null)
        {
            activePlatePrefab = defaultPlatePrefab;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            CheckSpawnPoints();
        }
    }

    public void SpawnBatch()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].childCount == 0)
            {
                GameObject plateObj = Instantiate(activePlatePrefab, spawnPoints[i]);
                plateObj.transform.localPosition = Vector3.zero;

                Plate plate = plateObj.GetComponent<Plate>();
                SpawnPizzaOnPlate(plate);
            }
        }
    }

    public void RefreshSkin()
    {
        PlayerData data = SaveSystem.Load();
        activePlatePrefab = Resources.Load<GameObject>(data.equippedSkinPrefab);

        if (activePlatePrefab == null)
            activePlatePrefab = defaultPlatePrefab;

        Debug.Log("Plate skin refreshed.");
    }

    void SpawnPizzaOnPlate(Plate plate)
    {
        if (plate == null || plate.slots == null || plate.slots.Length == 0)
            return;

        int pizzaCount = Random.Range(1, 4); // spawn 1–3 pizzas

        // sequential slot assignment (slot0 → slot1 → slot2…)
        for (int i = 0; i < pizzaCount; i++)
        {
            int randomIndex = Random.Range(0, pizzaPrefabs.Count);
            GameObject pizza = Instantiate(pizzaPrefabs[randomIndex]);

            Transform slot = plate.slots[i];
            pizza.transform.SetParent(slot);
            pizza.transform.localPosition = Vector3.zero;
            pizza.transform.localRotation = Quaternion.identity;
        }

        // optional: check if plate is pure right after spawn
        if (plate.IsPure())
        {
            Debug.Log($"{plate.name} spawned as PURE ({plate.slots[0].GetChild(0).tag})");
        }
    }

    public void CheckSpawnPoints()
    {
        bool allEmpty = true;
        foreach (Transform sp in spawnPoints)
        {
            if (sp.childCount > 0)
            {
                allEmpty = false;
                break;
            }
        }

        if (allEmpty)
        {
            SpawnBatch();
        }
    }
}
