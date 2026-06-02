using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlateSpawner : MonoBehaviour
{
    public static PlateSpawner Instance;

    public GameObject platePrefab;
    public List<GameObject> pizzaPrefabs; // Pizza1..Pizza6
    public Transform[] spawnPoints;       // 3 spawn point gán trong Inspector

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Nếu đang ở Playing thì luôn kiểm tra spawn point
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            CheckSpawnPoints();
        }
    }

    public void SpawnBatch()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i].childCount == 0) // chỉ spawn nếu trống
            {
                GameObject plateObj = Instantiate(platePrefab, spawnPoints[i]);
                plateObj.transform.localPosition = Vector3.zero;
                SpawnPizzaOnPlate(plateObj.transform);
            }
        }
    }

    void SpawnPizzaOnPlate(Transform plateTransform)
    {
        int pizzaCount = Random.Range(1, 4); // spawn 1–3 pizzas
        float height = 0.2f;                 // fixed height above plate
        float radius = 0f;                 // distance from center

        // Precompute 6 slot positions and rotations
        Vector3[] slots = new Vector3[6];
        Quaternion[] rotations = new Quaternion[6];

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f; // 0, 60, 120, 180, 240, 300
            slots[i] = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                height,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            // Rotate so pizza faces outward from center
            rotations[i] = Quaternion.Euler(0f, angle, 0f);
        }

        // Pick random slots for pizzas
        List<int> availableSlots = Enumerable.Range(0, 6).ToList();
        for (int i = 0; i < pizzaCount; i++)
        {
            int randomIndex = Random.Range(0, pizzaPrefabs.Count);
            GameObject pizza = Instantiate(pizzaPrefabs[randomIndex], plateTransform);

            // Choose a random free slot
            int slotIndex = availableSlots[Random.Range(0, availableSlots.Count)];
            availableSlots.Remove(slotIndex);

            pizza.transform.localPosition = slots[slotIndex];
            pizza.transform.localRotation = rotations[slotIndex];
        }

        // Optional: check if plate is pure right after spawn
        Plate plate = plateTransform.GetComponent<Plate>();
        if (plate != null && plate.IsPure())
        {
            Debug.Log($"{plate.name} spawned as PURE ({plate.transform.GetChild(0).tag})");
        }
    }

    public void CheckSpawnPoints()
    {
        bool allEmpty = true;
        foreach (Transform sp in spawnPoints)
        {
            if (sp.childCount > 0) { allEmpty = false; break; }
        }

        if (allEmpty)
        {
            SpawnBatch();
        }
    }
}
