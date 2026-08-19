using UnityEngine;
using System.Collections.Generic;

public class HumanSpawnSystem : MonoBehaviour
{
    [Header("스폰할 Human 프리팹")]
    [SerializeField] private GameObject humanPrefab;

    private HumanSpawner spawner;

    private void Awake()
    {
        spawner = new HumanSpawner(humanPrefab);
    }

    public GameObject SpawnHuman(Vector3 position, Quaternion rotation, string personName = "")
    {
        return spawner.Spawn(position, rotation, personName);
    }

    public GameObject SpawnHuman(Vector3 position, Quaternion rotation, PersonIdentity identity)
    {
        return spawner.Spawn(position, rotation, identity);
    }

    public void DespawnHuman(GameObject human)
    {
        spawner.Despawn(human);
    }

    public List<GameObject> GetSpawnedHumans()
    {
        return spawner.GetSpawnedHumans();
    }
}
