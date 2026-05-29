using UnityEngine;
using System.Collections;

public class PizzaMover : MonoBehaviour
{
    public static PizzaMover instance;
    public float moveDuration = 0.8f;

    private void Awake()
    {
        instance = this;
    }

    public void MovePizza(Transform pizza, Vector3 startPos, Vector3 endPos, Plate source, Plate target, System.Action onComplete)
    {
        // ✅ If pizza is already in the target plate AND already at its final spot, skip animation
        if (pizza.parent == target.transform &&
            Vector3.Distance(pizza.localPosition, target.transform.InverseTransformPoint(endPos)) < 0.001f)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(MoveAlongBezier(pizza, startPos, endPos, source, target, onComplete));
    }

    IEnumerator MoveAlongBezier(Transform pizza, Vector3 startPos, Vector3 endPos, Plate source, Plate target, System.Action onComplete)
    {
        pizza.SetParent(null);

        // Start and end at plate height
        startPos.y = 0.18f;
        endPos.y = 0.18f;

        // Control point above midpoint for arc
        Vector3 controlPoint = (startPos + endPos) / 2f;
        controlPoint.y = 2f; // arc height

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            // Quadratic Bezier interpolation
            Vector3 pos = Mathf.Pow(1 - t, 2) * startPos +
                          2 * (1 - t) * t * controlPoint +
                          Mathf.Pow(t, 2) * endPos;

            pizza.position = pos;
            yield return null;
        }

        // ✅ Check lock before re-parenting
        if (target.IsLockedToSingleType(out string lockedTag) && pizza.tag != lockedTag)
        {
            // Wrong type → send back to source
            pizza.SetParent(source.transform);
            pizza.localPosition = Vector3.up * ((source.transform.childCount - 1) * 0.2f);
            Debug.Log($"Pizza {pizza.tag} rejected by {target.name}, sent back to {source.name}");
        }
        else
        {
            // ✅ Capacity check before placement
            if (target.IsFull())
            {
                // Target full → send back to source
                pizza.SetParent(source.transform);
                pizza.localPosition = Vector3.up * ((source.transform.childCount - 1) * 0.2f);
                Debug.Log($"Target {target.name} is full, pizza returned to {source.name}");
            }
            else
            {
                // 🍕 Proper pizza wheel formation
                pizza.SetParent(target.transform);

                int sliceCount = target.transform.childCount;
                float angleStep = 360f / sliceCount;

                // Radius = 0 → tips meet exactly at center
                float radius = 0f;

                // Re‑arrange ALL pizzas on the plate
                for (int i = 0; i < sliceCount; i++)
                {
                    Transform slice = target.transform.GetChild(i);

                    float angle = i * angleStep * Mathf.Deg2Rad;

                    // Position at center (no spacing)
                    Vector3 circlePos = new Vector3(
                        Mathf.Cos(angle) * radius,
                        0.18f,
                        Mathf.Sin(angle) * radius
                    );

                    slice.localPosition = circlePos;

                    // Rotate slice so crust faces outward, tip inward
                    slice.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 180f, 0f);
                }
            }
        }

        onComplete?.Invoke();
    }
}
