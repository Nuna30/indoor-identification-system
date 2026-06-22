using UnityEngine;

public class Human : MonoBehaviour
{
    [Header("블렌더 Human 에셋 설정")]
    [SerializeField] private GameObject humanAsset;

    public GameObject HumanAsset
    {
        get => humanAsset;
        set => humanAsset = value;
    }

    private void Start()
    {
        GameObject spawnedHuman = Instantiate(humanAsset, transform.position, transform.rotation, transform);
    }
}
