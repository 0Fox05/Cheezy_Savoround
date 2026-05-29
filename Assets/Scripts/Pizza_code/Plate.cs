using UnityEngine;

public class Plate : MonoBehaviour
{
    public int maxPizza = 6;

    // true after player places plate
    public bool isPlaced;

    // lower = newer
    public float placedTimer;

    private void Update()
    {
        if (isPlaced)
        {
            placedTimer += Time.deltaTime;
        }
    }

    public void OnPlaced()
    {
        isPlaced = true;

        // newest starts at 0
        placedTimer = 0f;
    }

    public bool IsFull()
    {
        return transform.childCount >= maxPizza;
    }

    public bool HasPizza(string tag)
    {
        foreach (Transform pizza in transform)
        {
            if (pizza.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }
    public bool IsMixed()
    {
        if (transform.childCount <= 1) return false;
        string firstTag = transform.GetChild(0).tag;
        for (int i = 1; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag != firstTag)
                return true;
        }
        return false;
    }

    public bool IsPure()
    {
        // empty plate is NOT pure
        if (transform.childCount == 0)
        {
            return false;
        }

        string firstTag =
            transform.GetChild(0).tag;

        for (int i = 1; i < transform.childCount; i++)
        {
            if (
                transform.GetChild(i).tag
                != firstTag
            )
            {
                return false;
            }
        }

        return true;
    }
    public bool IsLockedToSingleType(out string lockedTag)
    {
        lockedTag = null;

        if (transform.childCount == 0) return false;

        string firstTag = transform.GetChild(0).tag;
        for (int i = 1; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag != firstTag)
                return false; // still mixed
        }

        lockedTag = firstTag;
        return true; // locked to one type
    }

}