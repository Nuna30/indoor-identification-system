using UnityEngine;

public class Human : MonoBehaviour
{
    [Header("블렌더 Human 에셋 설정")]
    [SerializeField] private GameObject humanAsset;

    private GameObject spawnedInstance;

    public GameObject HumanAsset
    {
        get => humanAsset;
        set => humanAsset = value;
    }

    private void Awake()
    {
        EnsureInstanceSpawned();
    }

    private void EnsureInstanceSpawned()
    {
        if (spawnedInstance == null && humanAsset != null)
        {
            spawnedInstance = Instantiate(humanAsset, transform.position, transform.rotation, transform);
        }
    }

    public void SetColor(Color color)
    {
        EnsureInstanceSpawned();

        if (spawnedInstance != null)
        {
            // 모든 자식 Renderer들의 머티리얼 색상 변경
            Renderer[] renderers = spawnedInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                // material.color에 대입하면 개별 인스턴스 머티리얼이 생성되어 원본 에셋이 손상되지 않습니다.
                renderer.material.color = color;
            }
        }
    }
}
