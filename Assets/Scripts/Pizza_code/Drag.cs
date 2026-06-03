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

    void Start()
    {
        cam = Camera.main;
        plate = GetComponent<Plate>();
    }

    void Update()
    {
        // ✅ If not dragging or already placed, do nothing
        if (!dragging || plate.isPlaced)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            transform.position = point + offset;
        }
    }

    void OnMouseDown()
    {
        // ✅ Don’t allow picking up again once placed
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
        // If plate already placed, ignore completely
        if (plate.isPlaced)
            return;

        // If we never started dragging, ignore
        if (!dragging)
            return;

        dragging = false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            // Only allow snapping to blocks in Playing state
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
            {
                if (hit.collider.CompareTag("Block"))
                {
                    Plate existingPlate = hit.collider.GetComponentInChildren<Plate>();
                    if (existingPlate != null && existingPlate != plate && existingPlate.isPlaced)
                    {
                        Debug.LogWarning($"Block {hit.collider.name} already has a plate!");
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

        // ❌ Always revert to original position if not placed on a valid block
        transform.position = startPosition;
    }
}
