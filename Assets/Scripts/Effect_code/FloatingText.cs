using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public TMP_Text text;
    public float floatDuration = 1f;
    public float floatSpeed = 1f;

    public void Show(string message)
    {
        text.text = message;
        StartCoroutine(FloatRoutine());
    }

    IEnumerator FloatRoutine()
    {
        float t = 0f;
        Vector3 startPos = transform.position;

        while (t < floatDuration)
        {
            t += Time.deltaTime;

            // Offset upward in Y, while drifting forward in Z
            transform.position = startPos
                + Vector3.up * 0.5f                // 👈 small lift above plate
                + Vector3.forward * (t * floatSpeed); // 👈 main float direction

            yield return null;
        }

        PoolManager.Instance.scoreTextPool.Return(gameObject);
    }
}
