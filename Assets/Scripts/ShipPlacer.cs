using UnityEngine;

public class ShipPlacer : MonoBehaviour
{
    public static ShipPlacer Instance { get; private set; }

    [Header("Preview Settings")]
    public Material validPreviewMaterial;
    public Material invalidPreviewMaterial;

    private ShipData currentShipData;
    private GameObject previewObject;
    private bool isRotated = false; // false: 기본, true: 90도 회전

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

    private void Update()
    {
        if (currentShipData == null) return;

        HandleRotationInput();
        UpdatePreview();
        HandlePlacementInput();
    }

    public void StartPlacement(ShipData shipData)
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        currentShipData = shipData;
        isRotated = false;

        // 프리뷰 오브젝트 생성 (임시로 큐브 사용)
        previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 콜라이더 제거 (레이캐스트 방해 방지)
        Destroy(previewObject.GetComponent<Collider>());
        
        UpdatePreviewShape();
    }

    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
        {
            isRotated = !isRotated;
            UpdatePreviewShape();
        }
    }

    private void UpdatePreviewShape()
    {
        if (previewObject == null || currentShipData == null) return;

        int width = isRotated ? currentShipData.size.y : currentShipData.size.x;
        int height = isRotated ? currentShipData.size.x : currentShipData.size.y;

        // 크기 조정 (높이는 약간 띄움)
        previewObject.transform.localScale = new Vector3(width * GridManager.Instance.cellSize, 0.5f, height * GridManager.Instance.cellSize);
    }

    private void UpdatePreview()
    {
        if (previewObject == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(point);

            // 프리뷰 위치 업데이트 (그리드 셀에 맞춤)
            // 함선의 중심이 아닌, 시작 셀(좌측 하단) 기준으로 위치를 잡고 크기만큼 오프셋을 줌
            int width = isRotated ? currentShipData.size.y : currentShipData.size.x;
            int height = isRotated ? currentShipData.size.x : currentShipData.size.y;

            // 시각적 위치는 중심점이므로 보정 필요
            float xOffset = width * GridManager.Instance.cellSize * 0.5f;
            float zOffset = height * GridManager.Instance.cellSize * 0.5f;

            Vector3 snapPos = new Vector3(gridPos.x * GridManager.Instance.cellSize + xOffset, 0.5f, gridPos.y * GridManager.Instance.cellSize + zOffset);
            previewObject.transform.position = snapPos;

            // 유효성 검사 및 색상 변경
            bool isValid = GridManager.Instance.CanPlaceShip(gridPos.x, gridPos.y, width, height);
            Renderer renderer = previewObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = isValid ? validPreviewMaterial : invalidPreviewMaterial;
            }
        }
    }

    private void HandlePlacementInput()
    {
        // 우클릭: 취소
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
            return;
        }

        // 좌클릭: 배치 시도
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                Vector2Int gridPos = GridManager.Instance.WorldToGridPosition(point);

                int width = isRotated ? currentShipData.size.y : currentShipData.size.x;
                int height = isRotated ? currentShipData.size.x : currentShipData.size.y;

                if (GridManager.Instance.CanPlaceShip(gridPos.x, gridPos.y, width, height))
                {
                    GridManager.Instance.PlaceShip(gridPos.x, gridPos.y, width, height);
                    
                    // 실제 함선 오브젝트 생성 (여기서는 프리뷰를 그대로 두고 색상만 바꾸거나, 별도 프리팹 생성 가능)
                    // 현재 요구사항에는 "배치된다"라고만 되어 있으므로, 프리뷰 오브젝트를 그대로 남기고 배치 모드 종료 처리
                    // 실제로는 함선 프리팹을 인스턴스화 해야 함. 여기서는 시각적 확인을 위해 프리뷰 오브젝트를 활용.
                    
                    GameObject shipObj = Instantiate(previewObject);
                    shipObj.transform.position = previewObject.transform.position;
                    shipObj.transform.localScale = previewObject.transform.localScale;
                    // 배치된 함선은 기본 머티리얼이나 별도 머티리얼로 변경 가능
                    // 여기서는 유효한 색상(초록) 그대로 유지하거나 흰색 등으로 변경
                    shipObj.GetComponent<Renderer>().material = validPreviewMaterial; 

                    CancelPlacement(); // 배치 모드 종료
                }
                else
                {
                    // 배치 불가 피드백 (이미 빨간색 프리뷰로 표시 중)
                    Debug.Log("Cannot place ship here!");
                }
            }
        }
    }

    private void CancelPlacement()
    {
        currentShipData = null;
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }
}
