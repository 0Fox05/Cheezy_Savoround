using UnityEngine;
using System.Collections;

public class PizzaMover : MonoBehaviour
{
    public static PizzaMover instance;
    public float moveDuration = 2f;

    // Pre‑calculated 6 slots
    private Vector3[] slotPositions = new Vector3[6];
    private Quaternion[] slotRotations = new Quaternion[6];

    private void Awake()
    {
        instance = this;
        PrecalculateSlots();
    }

    private void PrecalculateSlots()
    {
        float radius = 0f; // distance from center
        float height = 0.18f;

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad; // 360/6 = 60°
            slotPositions[i] = new Vector3(
                Mathf.Cos(angle) * radius,
                height,
                Mathf.Sin(angle) * radius
            );
            slotRotations[i] = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 180f, 0f);
        }
    }

    public void MovePizza(Transform pizza, Vector3 startPos, Vector3 endPos, Plate source, Plate target, System.Action onComplete)
    {
        StartCoroutine(MoveAlongBezier(pizza, startPos, endPos, source, target, onComplete));
    }

    IEnumerator MoveAlongBezier(Transform pizza, Vector3 startPos, Vector3 endPos, Plate source, Plate target, System.Action onComplete)
    {
        // Re‑parent immediately
        pizza.SetParent(target.transform);

        startPos.y = 0.18f;
        endPos.y = 0.18f;

        // Midpoint between start and end
        Vector3 midPoint = (startPos + endPos) / 2f;

        // ✅ Dramatic sideways offset (always wide)
        float sideOffset = Random.Range(1.5f, 2.5f);
        if (Random.value < 0.5f) sideOffset = -sideOffset;

        // Control point is above and sideways → boomerang curve
        Vector3 controlPoint = midPoint + new Vector3(sideOffset, 2f, 0f);

        float elapsed = 0f;

        // Target slot index = last child index
        int targetIndex = target.transform.childCount;
        if (targetIndex >= slotPositions.Length) targetIndex = slotPositions.Length - 1;

        Vector3 finalLocalPos = slotPositions[targetIndex];
        Quaternion finalLocalRot = slotRotations[targetIndex];

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

            // ✅ Smooth rotation towards final slot rotation
            pizza.rotation = Quaternion.Slerp(startRot, target.transform.rotation * finalLocalRot, t);

            yield return null;
        }

        // ✅ Snap to exact slot position/rotation
        int sliceCount = target.transform.childCount;
        for (int i = 0; i < sliceCount; i++)
        {
            Transform slice = target.transform.GetChild(i);
            slice.localPosition = slotPositions[i];
            slice.localRotation = slotRotations[i];
        }

        onComplete?.Invoke();
    }

}
