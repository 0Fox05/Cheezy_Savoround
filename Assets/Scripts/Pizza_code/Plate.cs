using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Plate : MonoBehaviour
{
    public int maxPizza = 6;
    public bool isPlaced;
    public float placedTimer;
    public bool hasCleared = false;
    public bool isClearing = false;
    public int nextSlotIndex = 0; // start at slot 0

    public Transform[] slots; // assign in Inspector or generate dynamically

    public void ReplaceWithPrefab(string prefabName)
    {
        GameObject newPrefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (newPrefab != null)
        {
            Instantiate(newPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"Prefab {prefabName} not found in Resources/Prefabs!");
        }
    }

    private void Update()
    {
        if (isPlaced)
        {
            placedTimer += Time.deltaTime;
        }
    }

    public void OnPlaced()
    {
        placedTimer = 0f;

        FallAnimation fall = GetComponent<FallAnimation>();
        if (fall != null)
        {
            fall.PlayFall(() =>
            {
                isPlaced = true;
                GameManager.Instance.ChangeState(GameState.Sorting);
            });
        }
        else
        {
            isPlaced = true;
            GameManager.Instance.ChangeState(GameState.Sorting);
        }
    }

    // Helper: get all pizzas across slots
    public IEnumerable<Transform> GetPizzas()
    {
        if (slots == null) yield break;
        foreach (Transform slot in slots)
        {
            foreach (Transform pizza in slot)
                yield return pizza;
        }
    }

    public bool IsFull()
    {
        if (!isPlaced || slots == null) return false;
        return slots.All(s => s.childCount > 0);
    }

    public bool HasPizza(string tag)
    {
        if (!isPlaced) return false;
        return GetPizzas().Any(p => p.CompareTag(tag));
    }

    public bool IsMixed()
    {
        var pizzas = GetPizzas().ToList();
        if (!isPlaced || pizzas.Count <= 1) return false;
        string firstTag = pizzas[0].tag;
        return pizzas.Any(p => p.tag != firstTag);
    }

    public bool IsPure()
    {
        var pizzas = GetPizzas().ToList();
        if (!isPlaced || pizzas.Count == 0) return false;
        string firstTag = pizzas[0].tag;
        return pizzas.All(p => p.tag == firstTag);
    }

    public bool IsLockedToSingleType(out string lockedTag)
    {
        lockedTag = null;
        var pizzas = GetPizzas().ToList();
        if (!isPlaced || pizzas.Count == 0) return false;
        string firstTag = pizzas[0].tag;
        if (pizzas.All(p => p.tag == firstTag))
        {
            lockedTag = firstTag;
            return true;
        }
        return false;
    }
}
