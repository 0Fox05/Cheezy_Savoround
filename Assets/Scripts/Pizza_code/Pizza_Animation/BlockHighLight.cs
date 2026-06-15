using UnityEngine;

public class BlockHighlight : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;
    private Vector3 originalPos;

    [Header("Highlight Colors")]
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        originalPos = transform.position;
    }

    public void Highlight(bool isValid)
    {
        rend.material.color = isValid ? validColor : invalidColor;
        transform.position = originalPos + Vector3.up * 0.1f; // lift slightly
    }

    public void ResetHighlight()
    {
        rend.material.color = originalColor;
        transform.position = originalPos;
    }
}
