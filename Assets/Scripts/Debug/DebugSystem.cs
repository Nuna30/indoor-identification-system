using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugSystem : MonoBehaviour
{
    [Header("시스템 프리팹 설정")]
    [SerializeField] private HumanSpawnSystem humanSpawnSystemPrefab;
    [SerializeField] private SectionManager sectionManagerPrefab;

    private HumanSpawnSystem humanSpawnSystemInstance;
    private SectionManager sectionManagerInstance;
    
    private List<Section> sections = new List<Section>();
    private List<List<GameObject>> sectionHumans = new List<List<GameObject>>();

    private void Awake()
    {
        // 초기화 로직
    }

    private void Start()
    {
        InitializeInstances();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 추가(Spawn) 단축키: Q, W, E, R (각각 Section 1, 2, 3, 4)
        Key[] spawnKeys = new Key[] { Key.Q, Key.W, Key.E, Key.R };
        // 제거(Despawn) 단축키: A, S, D, F (각각 Section 1, 2, 3, 4)
        Key[] despawnKeys = new Key[] { Key.A, Key.S, Key.D, Key.F };

        // 최대 4개의 Section에 대해 키 입력 감지
        int checkCount = Mathf.Min(sections.Count, 4);
        for (int i = 0; i < checkCount; i++)
        {
            // 사람 추가 (Q, W, E, R)
            if (keyboard[spawnKeys[i]].wasPressedThisFrame)
            {
                SpawnHumanInSection(i);
            }
            // 사람 제거 (A, S, D, F)
            if (keyboard[despawnKeys[i]].wasPressedThisFrame)
            {
                DespawnHumanFromSection(i);
            }
        }
    }

    private void InitializeInstances()
    {
        // 씬에서 인스턴스 탐색
        humanSpawnSystemInstance = FindObjectOfType<HumanSpawnSystem>();
        if (humanSpawnSystemInstance == null && humanSpawnSystemPrefab != null)
        {
            humanSpawnSystemInstance = Instantiate(humanSpawnSystemPrefab);
        }

        sectionManagerInstance = FindObjectOfType<SectionManager>();
        if (sectionManagerInstance == null && sectionManagerPrefab != null)
        {
            sectionManagerInstance = Instantiate(sectionManagerPrefab);
        }

        // Section들 탐색 및 생성
        Section[] existingSections = FindObjectsOfType<Section>();
        if (existingSections.Length > 0)
        {
            // 이름 순 정렬하여 sections 리스트에 추가 (예: Section 1, Section 2...)
            System.Array.Sort(existingSections, (a, b) => string.Compare(a.name, b.name));
            sections.AddRange(existingSections);
        }
        else if (sectionManagerInstance != null)
        {
            // 씬에 Section이 없다면 SectionManager 프리팹 목록을 바탕으로 인스턴스화
            for (int i = 0; i < sectionManagerInstance.SectionPrefabs.Count; i++)
            {
                GameObject prefab = sectionManagerInstance.SectionPrefabs[i];
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab);
                    Section sectionComp = obj.GetComponent<Section>();
                    if (sectionComp != null)
                    {
                        sections.Add(sectionComp);
                    }
                }
            }
        }

        // 각 Section 마다 Human 리스트 초기화
        for (int i = 0; i < sections.Count; i++)
        {
            sectionHumans.Add(new List<GameObject>());
        }
    }

    private void SpawnHumanInSection(int sectionIndex)
    {
        if (sectionIndex < 0 || sectionIndex >= sections.Count) return;

        Section targetSection = sections[sectionIndex];
        List<GameObject> targetHumans = sectionHumans[sectionIndex];

        int newCount = targetHumans.Count + 1;
        Vector3[] targetPositions = targetSection.GetSpawnPositions(newCount);

        // 기존 Human들 위치 정렬 업데이트
        for (int i = 0; i < targetHumans.Count; i++)
        {
            if (targetHumans[i] != null)
            {
                targetHumans[i].transform.position = targetPositions[i];
            }
        }

        // 신규 Human 스폰 및 리스트 추가 (마지막 위치)
        GameObject newHuman = humanSpawnSystemInstance.SpawnHuman(targetPositions[newCount - 1], Quaternion.identity);
        if (newHuman != null)
        {
            targetHumans.Add(newHuman);
        }
    }

    private void DespawnHumanFromSection(int sectionIndex)
    {
        if (sectionIndex < 0 || sectionIndex >= sections.Count) return;

        Section targetSection = sections[sectionIndex];
        List<GameObject> targetHumans = sectionHumans[sectionIndex];

        // 마지막 Human 가져오기 및 제거
        int lastIndex = targetHumans.Count - 1;
        GameObject lastHuman = targetHumans[lastIndex];

        if (lastHuman != null)
        {
            humanSpawnSystemInstance.DespawnHuman(lastHuman);
        }
        targetHumans.RemoveAt(lastIndex);

        // 남은 Human들 위치 정렬 업데이트
        int newCount = targetHumans.Count;
        if (newCount > 0)
        {
            Vector3[] targetPositions = targetSection.GetSpawnPositions(newCount);
            for (int i = 0; i < newCount; i++)
            {
                if (targetHumans[i] != null)
                {
                    targetHumans[i].transform.position = targetPositions[i];
                }
            }
        }
    }
}
