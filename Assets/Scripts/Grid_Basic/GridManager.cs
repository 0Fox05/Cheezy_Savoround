using UnityEngine;
using System.IO;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    private GridData grid;

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "grid.json");
        string json = File.ReadAllText(path);
        grid = JsonUtility.FromJson<GridData>(json);

        foreach (Tile tile in grid.tiles)
        {
            Vector3 pos = new Vector3(
                tile.x * grid.cellSize,
                0,
                tile.y * grid.cellSize
            );

            GameObject obj = Instantiate(tilePrefab, pos, Quaternion.identity);

            Renderer rend = obj.GetComponent<Renderer>();

            if ((tile.x + tile.y) % 2 == 0)
            {
                rend.material.color = new Color(0.96f, 0.92f, 0.84f);
            }
            else
            {
                rend.material.color = new Color(0.90f, 0.84f, 0.72f);
            }
        }
    }
}
