using UnityEngine;
using System.Collections;

public class FallAnimation : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallDistance = 0.5f;   // how far above the tile it starts
    public float fallDuration = 0.25f;  // how long the fall takes

    private bool isAnimating;

    public void PlayFall(System.Action onComplete = null)
    {
        if (!isAnimating)
            StartCoroutine(FallRoutine(onComplete));
    }

    private IEnumerator FallRoutine(System.Action onComplete)
    {
        isAnimating = true;

        Vector3 targetPos = transform.localPosition;
        Vector3 startPos = targetPos + Vector3.up * fallDistance;

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            float t = elapsed / fallDuration;
            // Smooth fall using Lerp
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
        isAnimating = false;

        onComplete?.Invoke();
    }
}
