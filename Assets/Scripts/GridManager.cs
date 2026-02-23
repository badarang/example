using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1.0f;

    [Header("References")]
    public GameObject tilePrefab; // 그리드 시각화를 위한 타일 프리팹

    private bool[,] grid; // true: 함선 있음, false: 비어있음

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        grid = new bool[width, height];

        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile Prefab is not assigned in GridManager.");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, transform);
                tile.transform.position = new Vector3(x * cellSize, 0, y * cellSize);
                tile.name = $"Tile_{x}_{y}";
            }
        }
    }

    // 월드 좌표를 그리드 좌표로 변환
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, y);
    }

    // 그리드 좌표를 월드 좌표로 변환 (셀의 중심)
    public Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize + cellSize * 0.5f, 0, y * cellSize + cellSize * 0.5f);
    }

    // 배치 가능 여부 확인
    public bool CanPlaceShip(int startX, int startY, int sizeX, int sizeY)
    {
        // 그리드 범위 체크
        if (startX < 0 || startY < 0 || startX + sizeX > width || startY + sizeY > height)
        {
            return false;
        }

        // 이미 배치된 함선이 있는지 체크
        for (int x = startX; x < startX + sizeX; x++)
        {
            for (int y = startY; y < startY + sizeY; y++)
            {
                if (grid[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    // 함선 배치 확정
    public void PlaceShip(int startX, int startY, int sizeX, int sizeY)
    {
        for (int x = startX; x < startX + sizeX; x++)
        {
            for (int y = startY; y < startY + sizeY; y++)
            {
                grid[x, y] = true;
            }
        }
    }
}
