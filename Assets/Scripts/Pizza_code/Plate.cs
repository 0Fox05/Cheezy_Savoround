using UnityEngine;
using System.Linq;

public class Plate : MonoBehaviour
{
    public int maxPizza = 6;
    public bool isPlaced;
    public float placedTimer;

    private void Update()
    {
        if (isPlaced)
        {
            placedTimer += Time.deltaTime; // continuously increases
        }
    }

    public void OnPlaced()
    {
        isPlaced = true;
        placedTimer = 0f; // reset to zero when placed
        GameManager.Instance.ChangeState(GameState.Sorting);
    }

    public bool IsFull()
    {
        return isPlaced && transform.childCount >= maxPizza;
    }

    public bool HasPizza(string tag)
    {
        if (!isPlaced) return false;
        foreach (Transform pizza in transform)
        {
            if (pizza.CompareTag(tag)) return true;
        }
        return false;
    }

    public bool IsMixed()
    {
        if (!isPlaced || transform.childCount <= 1) return false;

        string firstTag = transform.GetChild(0).tag;
        foreach (Transform pizza in transform)
        {
            if (pizza.tag != firstTag) return true;
        }
        return false;
    }

    public bool IsPure()
    {
        if (!isPlaced || transform.childCount == 0) return false;

        string firstTag = transform.GetChild(0).tag;
        foreach (Transform pizza in transform)
        {
            if (pizza.tag != firstTag) return false;
        }
        return true;
    }

    public bool IsLockedToSingleType(out string lockedTag)
    {
        lockedTag = null;
        if (!isPlaced || transform.childCount == 0) return false;

        string firstTag = transform.GetChild(0).tag;
        foreach (Transform pizza in transform)
        {
            if (pizza.tag != firstTag) return false;
        }

        lockedTag = firstTag;
        return true;
    }
}
