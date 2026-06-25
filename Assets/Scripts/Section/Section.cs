using UnityEngine;

public class Section : MonoBehaviour
{
    [Header("직사각형 영역의 4개 꼭짓점 좌표 (X, Z)")]
    [Tooltip("직사각형을 구성하는 4개의 꼭짓점 좌표입니다. Vector2의 (x, y)를 각각 (x, z)로 취급합니다.")]
    [SerializeField] private Vector2[] coordinates = new Vector2[4]
    {
        new Vector2(-5f, -5f),
        new Vector2(5f, -5f),
        new Vector2(5f, 5f),
        new Vector2(-5f, 5f)
    };

    [Header("Human 스폰 설정")]
    [Tooltip("배치할 Human 간의 간격을 설정합니다.")]
    [SerializeField] private float humanSpacing = 0.6f;

    [Tooltip("영역 경계로부터 떨어질 최소 마진을 설정합니다.")]
    [SerializeField] private float boundaryMargin = 0.6f;

    [Tooltip("사람들이 겹치지 않게 유지할 최소 간격을 설정합니다.")]
    [SerializeField] private float minSpacing = 0.25f;

    /// <summary>
    /// 직사각형을 구성하는 4개의 꼭짓점 좌표 배열입니다. (x, y)는 각각 (x, z) 평면 좌표를 의미합니다.
    /// </summary>
    public Vector2[] Coordinates
    {
        get => coordinates;
        set => coordinates = value;
    }

    private void Start()
    {
        UpdatePositionToCenter();
    }

    private void OnValidate()
    {
        UpdatePositionToCenter();
    }

    /// <summary>
    /// 4개 꼭짓점 좌표의 중심점을 계산하여 Section의 Transform 위치를 이동시킵니다.
    /// Vector2의 y 성분을 3D 공간의 z 성분으로 매핑합니다.
    /// </summary>
    public void UpdatePositionToCenter()
    {
        if (coordinates == null || coordinates.Length != 4) return;

        float sumX = 0f;
        float sumY = 0f; // Vector2의 y 성분을 z 좌표 계산용으로 사용
        foreach (var coord in coordinates)
        {
            sumX += coord.x;
            sumY += coord.y;
        }

        Vector3 center = new Vector3(sumX / 4f, transform.position.y, sumY / 4f);
        transform.position = center;
    }

    /// <summary>
    /// Section 영역의 가로 길이를 가져옵니다.
    /// </summary>
    public float GetSectionWidth()
    {
        if (coordinates == null || coordinates.Length != 4) return 10f;
        return Vector2.Distance(coordinates[0], coordinates[1]);
    }

    /// <summary>
    /// Section 영역의 세로(깊이) 길이를 가져옵니다.
    /// </summary>
    public float GetSectionHeight()
    {
        if (coordinates == null || coordinates.Length != 4) return 10f;
        return Vector2.Distance(coordinates[1], coordinates[2]);
    }

    /// <summary>
    /// 지정된 인원 수(n)에 맞게 겹치지 않고 중앙 정렬된 스폰 좌표들을 반환합니다.
    /// </summary>
    public Vector3[] GetSpawnPositions(int count)
    {
        if (count <= 0) return new Vector3[0];

        int cols = 1;
        int rows = 1;

        if (count == 1) { cols = 1; rows = 1; }
        else if (count == 2) { cols = 2; rows = 1; }
        else if (count == 3) { cols = 3; rows = 1; }
        else if (count == 4) { cols = 2; rows = 2; }
        else
        {
            cols = Mathf.CeilToInt(Mathf.Sqrt(count));
            rows = Mathf.CeilToInt((float)count / cols);
        }

        // 영역 크기와 마진에 맞춰 동적 간격(Spacing) 계산
        float areaWidth = GetSectionWidth();
        float areaHeight = GetSectionHeight();

        float maxAllowedWidth = Mathf.Max(0.01f, areaWidth - 2 * boundaryMargin);
        float maxAllowedHeight = Mathf.Max(0.01f, areaHeight - 2 * boundaryMargin);

        float activeSpacing = humanSpacing;

        // X축 방향 간격 제한 적용
        if (cols > 1)
        {
            float maxSpacingX = maxAllowedWidth / (cols - 1);
            activeSpacing = Mathf.Min(activeSpacing, maxSpacingX);
        }

        // Z축 방향 간격 제한 적용
        if (rows > 1)
        {
            float maxSpacingZ = maxAllowedHeight / (rows - 1);
            activeSpacing = Mathf.Min(activeSpacing, maxSpacingZ);
        }

        // 사람들이 너무 밀착해서 겹치지 않게 최소 간격 보장
        activeSpacing = Mathf.Max(activeSpacing, minSpacing);

        Vector3[] positions = new Vector3[count];
        Vector3 center = transform.position;

        for (int i = 0; i < count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            // 현재 행(row)에 배치될 열(column)의 수를 계산하여 중앙 정렬에 사용
            int colsInThisRow = cols;
            if (row == rows - 1)
            {
                colsInThisRow = count - (row * cols);
            }

            float xOffset = (col - (colsInThisRow - 1) / 2f) * activeSpacing;
            float zOffset = (row - (rows - 1) / 2f) * activeSpacing;

            // Section 오브젝트의 회전값을 반영하여 로컬 오프셋을 월드 오프셋으로 변환
            Vector3 localOffset = new Vector3(xOffset, 0, zOffset);
            positions[i] = center + transform.rotation * localOffset;
        }

        return positions;
    }
}
