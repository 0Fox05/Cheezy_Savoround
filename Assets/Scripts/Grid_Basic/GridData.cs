[System.Serializable]
public class Tile
{
    public int x;
    public int y;
    public string type;
}

[System.Serializable]
public class GridData
{
    public int gridWidth;
    public int gridHeight;
    public float cellSize;
    public Tile[] tiles;
}
