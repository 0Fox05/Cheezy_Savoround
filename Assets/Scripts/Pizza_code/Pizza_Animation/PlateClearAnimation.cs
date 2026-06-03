using UnityEngine;
using System.Collections;

public class PlateClearAnimation : MonoBehaviour
{
    public float growScale = 0.8f;
    public float growDuration = 0.3f;
    public float shrinkDuration = 1f;
    public float spinSpeed = 360f; // degrees per second
    public float animationLength = 1.3f;

    public void PlayClearAnimation()
    {
        var plate = GetComponent<Plate>();
        plate.isClearing = true;

        // run animation and reset at the end
        StartCoroutine(ClearRoutine(plate));
    }

    IEnumerator ClearRoutine(Plate plate)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * growScale;

        float t = 0f;

        // 🔄 Grow + spin CCW
        while (t < growDuration)
        {
            t += Time.deltaTime;
            float progress = t / growDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            yield return null;
        }

        t = 0f;

        // 🔄 Shrink + spin CW
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float progress = t / shrinkDuration;
            transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, progress);
            transform.Rotate(Vector3.down, spinSpeed * Time.deltaTime);
            yield return null;
        }

        // ✅ Reset before destroy (important if pooling)
        plate.isClearing = false;

        // 💥 Destroy after animation
        Destroy(gameObject);
    }
}
