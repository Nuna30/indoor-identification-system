using UnityEngine;
using System.Collections.Generic;

public class HumanSpawner
{
    private GameObject humanPrefab;
    private List<GameObject> spawnedHumans = new List<GameObject>();

    // 생성자를 통해 스폰할 프리팹을 주입받습니다.
    public HumanSpawner(GameObject prefab)
    {
        this.humanPrefab = prefab;
    }

    /// <summary>
    /// 실제 스폰 로직을 수행합니다. (MonoBehaviour 상속 없이도 Object.Instantiate 호출이 가능합니다.)
    /// </summary>
    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        if (humanPrefab == null)
        {
            Debug.LogWarning("HumanSpawner: 지정된 프리팹이 없어 스폰할 수 없습니다.");
            return null;
        }

        GameObject newHuman = Object.Instantiate(humanPrefab, position, rotation);
        spawnedHumans.Add(newHuman);
        return newHuman;
    }

    /// <summary>
    /// 실제 데스폰 로직을 수행합니다.
    /// </summary>
    public void Despawn(GameObject human)
    {
        if (human == null) return;

        if (spawnedHumans.Contains(human))
        {
            spawnedHumans.Remove(human);
        }

        Object.Destroy(human);
    }

    /// <summary>
    /// 생성되어 관리 중인 모든 Human 목록을 가져옵니다.
    /// </summary>
    public List<GameObject> GetSpawnedHumans()
    {
        return spawnedHumans;
    }
}
