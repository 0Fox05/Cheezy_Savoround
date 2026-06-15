using UnityEngine;

public class Drag : MonoBehaviour
{
    public LayerMask tileLayer;

    private Camera cam;
    private bool dragging;
    private Vector3 offset;
    private Vector3 startPosition;
    private Plane dragPlane;
    private Plate plate;

    private BlockHighlight currentHighlight;

    void Start()
    {
        cam = Camera.main;
        plate = GetComponent<Plate>();
    }

    void Update()
    {
        if (!dragging || plate.isPlaced)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            transform.position = point + offset;
        }

        // Highlight block under cursor
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
        {
            if (hit.collider.CompareTag("Block"))
            {
                Plate existingPlate = hit.collider.GetComponentInChildren<Plate>();
                bool isValid = (existingPlate == null || !existingPlate.isPlaced);

                BlockHighlight bh = hit.collider.GetComponent<BlockHighlight>();
                if (bh != null)
                {
                    if (currentHighlight != null && currentHighlight != bh)
                        currentHighlight.ResetHighlight();

                    bh.Highlight(isValid);
                    currentHighlight = bh;
                }
            }
        }
        else
        {
            if (currentHighlight != null)
            {
                currentHighlight.ResetHighlight();
                currentHighlight = null;
            }
        }
    }

    void OnMouseDown()
    {
        if (plate.isPlaced)
            return;

        dragging = true;
        startPosition = transform.position;

        dragPlane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            offset = transform.position - ray.GetPoint(distance);
        }
    }

    void OnMouseUp()
    {
        if (currentHighlight != null)
        {
            currentHighlight.ResetHighlight();
            currentHighlight = null;
        }

        if (plate.isPlaced)
            return;

        if (!dragging)
            return;

        dragging = false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
            {
                if (hit.collider.CompareTag("Block"))
                {
                    Plate existingPlate = hit.collider.GetComponentInChildren<Plate>();
                    if (existingPlate != null && existingPlate != plate && existingPlate.isPlaced)
                    {
                        Debug.Log($"Block {hit.collider.name} already has a plate!");
                        transform.position = startPosition; // revert
                        return;
                    }

                    Vector3 pos = hit.collider.transform.position;
                    pos.x = Mathf.Round(pos.x);
                    pos.z = Mathf.Round(pos.z);

                    Renderer blockRenderer = hit.collider.GetComponent<Renderer>();
                    Renderer myRenderer = GetComponentInChildren<Renderer>();

                    if (blockRenderer != null && myRenderer != null)
                    {
                        pos.y = blockRenderer.bounds.max.y + (myRenderer.bounds.size.y * 0.5f);
                    }

                    transform.position = pos;
                    transform.SetParent(hit.collider.transform);
                    plate.OnPlaced();
                    return;
                }
            }
        }

        transform.position = startPosition;
    }
}
