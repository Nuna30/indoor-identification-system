using UnityEngine;
using UnityEngine.Serialization;

public enum PersonIdentity
{
    Default,
    Hanbyul,   // 한별
    Minkyung,  // 민경
    Minhyeon,  // 민현
    Assistant  // 조교
}

public class Human : MonoBehaviour
{
    [Header("기본 Human 모델 에셋")]
    [FormerlySerializedAs("humanAsset")]
    [SerializeField] private GameObject defaultHumanAsset;

    [Header("인물별 전용 모델 에셋")]
    [SerializeField] private GameObject hanbyulAsset;   // 한별
    [SerializeField] private GameObject minkyungAsset;  // 민경
    [SerializeField] private GameObject minhyeonAsset;  // 민현
    [SerializeField] private GameObject assistantAsset; // 조교

    private GameObject spawnedInstance;
    private GameObject currentAssetUsed;
    private PersonIdentity currentPersonIdentity = PersonIdentity.Default;

    public PersonIdentity CurrentPersonIdentity => currentPersonIdentity;

    public GameObject DefaultHumanAsset
    {
        get => defaultHumanAsset;
        set => defaultHumanAsset = value;
    }

    public GameObject HanbyulAsset
    {
        get => hanbyulAsset;
        set => hanbyulAsset = value;
    }

    public GameObject MinkyungAsset
    {
        get => minkyungAsset;
        set => minkyungAsset = value;
    }

    public GameObject MinhyeonAsset
    {
        get => minhyeonAsset;
        set => minhyeonAsset = value;
    }

    public GameObject AssistantAsset
    {
        get => assistantAsset;
        set => assistantAsset = value;
    }

    private void Awake()
    {
        EnsureInstanceSpawned();
    }

    /// <summary>
    /// 인물 이름(한글/영문)에 따라 해당하는 3D 에셋으로 모델을 생성 및 교체합니다.
    /// </summary>
    public void SetPerson(string personName)
    {
        PersonIdentity identity = ParsePersonIdentity(personName);
        SetPerson(identity);
    }

    /// <summary>
    /// 인물 Identity 열거형에 따라 해당하는 3D 에셋으로 모델을 생성 및 교체합니다.
    /// </summary>
    public void SetPerson(PersonIdentity identity)
    {
        currentPersonIdentity = identity;
        GameObject targetAsset = GetAssetByIdentity(identity);
        if (targetAsset == null) targetAsset = defaultHumanAsset;

        UpdateSpawnedModel(targetAsset);
    }

    /// <summary>
    /// 특정 GameObject 에셋으로 모델을 직접 교체합니다.
    /// </summary>
    public void SetHumanAsset(GameObject newAsset)
    {
        UpdateSpawnedModel(newAsset);
    }

    private void EnsureInstanceSpawned()
    {
        if (spawnedInstance == null)
        {
            GameObject targetAsset = GetAssetByIdentity(currentPersonIdentity);
            if (targetAsset == null) targetAsset = defaultHumanAsset;
            if (targetAsset != null) UpdateSpawnedModel(targetAsset);
        }
    }

    private void UpdateSpawnedModel(GameObject targetAsset)
    {
        if (targetAsset == null) return;
        if (spawnedInstance != null && currentAssetUsed == targetAsset) return;

        if (spawnedInstance != null)
        {
            Destroy(spawnedInstance);
            spawnedInstance = null;
        }

        spawnedInstance = Instantiate(targetAsset, transform);
        spawnedInstance.transform.localPosition = Vector3.zero;
        spawnedInstance.transform.localRotation = Quaternion.identity;
        currentAssetUsed = targetAsset;
    }

    private GameObject GetAssetByIdentity(PersonIdentity identity)
    {
        return identity switch
        {
            PersonIdentity.Hanbyul => hanbyulAsset != null ? hanbyulAsset : defaultHumanAsset,
            PersonIdentity.Minkyung => minkyungAsset != null ? minkyungAsset : defaultHumanAsset,
            PersonIdentity.Minhyeon => minhyeonAsset != null ? minhyeonAsset : defaultHumanAsset,
            PersonIdentity.Assistant => assistantAsset != null ? assistantAsset : defaultHumanAsset,
            _ => defaultHumanAsset
        };
    }

    public static PersonIdentity ParsePersonIdentity(string name)
    {
        if (string.IsNullOrEmpty(name)) return PersonIdentity.Default;

        string normalized = name.ToLower().Trim();

        if (normalized.Contains("한별") || normalized.Contains("hanbyul") || normalized.Contains("hanbyeol")) return PersonIdentity.Hanbyul;
        else if (normalized.Contains("민경") || normalized.Contains("minkyung") || normalized.Contains("minkyoung") || normalized.Contains("minkyeong")) return PersonIdentity.Minkyung;
        else if (normalized.Contains("민현") || normalized.Contains("minhyeon") || normalized.Contains("minhyun")) return PersonIdentity.Minhyeon;
        else if (normalized.Contains("조교") || normalized.Contains("ta") || normalized.Contains("assistant") || normalized.Contains("instructor")) return PersonIdentity.Assistant;

        return PersonIdentity.Default;
    }

    public void SetColor(Color color)
    {
        EnsureInstanceSpawned();

        if (spawnedInstance != null)
        {
            Renderer[] renderers = spawnedInstance.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) renderer.material.color = color;
        }
    }
}
