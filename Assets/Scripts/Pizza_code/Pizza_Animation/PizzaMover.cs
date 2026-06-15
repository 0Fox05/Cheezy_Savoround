using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PizzaMover : MonoBehaviour
{
    public static PizzaMover instance;
    public float moveDuration = 0.6f;

    private void Awake()
    {
        instance = this;
    }

    public void MovePizza(Transform pizza, Plate source, Plate target, System.Action onComplete)
    {
        // Reserve the next slot strictly by index order
        Transform slot = ReserveNextOrderedSlot(target);

        if (slot == null)
        {
            Debug.LogWarning("No empty slot available on target plate!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(MoveAlongBezier(pizza, source.transform.position, slot.position, slot, onComplete));
    }

    private Transform ReserveNextOrderedSlot(Plate target)
    {
        for (int i = target.nextSlotIndex; i < target.slots.Length; i++)
        {
            Transform slot = target.slots[i];
            bool reserved = false;
            foreach (Transform child in slot)
            {
                if (child.name == "SlotReservation")
                {
                    reserved = true;
                    break;
                }
            }

            if (slot.childCount == 0 && !reserved)
            {
                GameObject placeholder = new GameObject("SlotReservation");
                placeholder.transform.SetParent(slot);
                placeholder.transform.localPosition = Vector3.zero;

                // Update tracker for next time
                target.nextSlotIndex = (i + 1) % target.slots.Length;
                return slot;
            }
        }

        // If no slot found, reset and retry from 0
        target.nextSlotIndex = 0;
        return null;
    }

    IEnumerator MoveAlongBezier(Transform pizza, Vector3 startPos, Vector3 endPos, Transform slot, System.Action onComplete)
    {
        pizza.SetParent(null); // detach for smooth world movement

        Vector3 midPoint = (startPos + endPos) / 2f;
        float sideOffset = Random.Range(1.5f, 2.5f);
        if (Random.value < 0.5f) sideOffset = -sideOffset;
        Vector3 controlPoint = midPoint + new Vector3(sideOffset, 2f, 0f);

        float elapsed = 0f;
        Quaternion startRot = pizza.rotation;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            // Quadratic Bezier interpolation
            Vector3 pos = Mathf.Pow(1 - t, 2) * startPos +
                          2 * (1 - t) * t * controlPoint +
                          Mathf.Pow(t, 2) * endPos;

            pizza.position = pos;
            pizza.rotation = Quaternion.Slerp(startRot, slot.rotation, t);

            yield return null;
        }

        // Snap to exact slot position/rotation
        pizza.SetParent(slot);
        pizza.localPosition = Vector3.zero;
        pizza.localRotation = Quaternion.identity;

        // Remove placeholder reservation
        foreach (Transform child in slot)
        {
            if (child.name == "SlotReservation")
            {
                Destroy(child.gameObject);
                break;
            }
        }

        onComplete?.Invoke();
    }
}
