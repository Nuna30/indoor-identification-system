using UnityEngine;
using System;
using System.Collections.Generic;
using NativeWebSocket;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[System.Serializable]
public class ZoneMapping
{
    public string zoneName;
    public Section targetSection; // 기존 아키텍처의 Section 컴포넌트 활용
    public Vector3 customPosition; // Section이 지정되지 않은 경우 코드/인스펙터 상에서 입력받을 기본 위치
}

public class Scenario1TestHumanSpawn : MonoBehaviour
{
    [Header("웹소켓 설정")]
    [SerializeField] private string serverUrl = "ws://127.0.0.1:8000/ws/csi";

    [Header("구역별 위치 및 Section 매핑 (코드 상에서 위치를 입력 및 수정 가능)")]
    private List<ZoneMapping> zoneMappings = new List<ZoneMapping>()
    {
        // 사용자가 코드 및 인스펙터상에서 위치를 제어할 수 있도록 기본값 사전 세팅
        new ZoneMapping { zoneName = "Zone 1", customPosition = new Vector3(1.37f, 0f, 3.39f) },
        new ZoneMapping { zoneName = "Zone 2", customPosition = new Vector3(2.74f, 0f, 3.39f) },
        new ZoneMapping { zoneName = "Zone 3", customPosition = new Vector3(0f, 0f, 3.39f) },
        new ZoneMapping { zoneName = "Zone 4", customPosition = new Vector3(6f, 0f, 0f) }
    };

    [Header("Human 스폰 시스템")]
    [SerializeField] private HumanSpawnSystem humanSpawnSystem;

    private WebSocket websocket;
    private Dictionary<string, GameObject> activeHumans = new Dictionary<string, GameObject>();

    private async void Start()
    {
        // 1. HumanSpawnSystem 캐싱 및 인스턴스 탐색
        if (humanSpawnSystem == null)
        {
            humanSpawnSystem = FindObjectOfType<HumanSpawnSystem>();
        }

        if (humanSpawnSystem == null)
        {
            Debug.LogError("Scenario1TestHumanSpawn: 씬에 HumanSpawnSystem이 없습니다. 컴포넌트를 등록해주세요.");
            return;
        }

        // 2. 파이썬 웹소켓 서버 연결 시작
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("📡 Scenario 1: 파이썬 CSI 추론 서버와 연결되었습니다!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"⚠️ 웹소켓 에러 발생: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("❌ 서버와의 연결이 해제되었습니다.");
        };

        websocket.OnMessage += (bytes) =>
        {
            string jsonText = System.Text.Encoding.UTF8.GetString(bytes);
            
            try
            {
                CSIInferenceData data = JsonUtility.FromJson<CSIInferenceData>(jsonText);
                ProcessInferenceResult(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 파싱 실패: {ex.Message}");
            }
        };

        await websocket.Connect();
    }

    private void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    #endif

        // 스페이스바 입력 감지 후 다음 패킷 요청
        bool isSpacePressed = false;
    #if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[Key.Space].wasPressedThisFrame)
        {
            isSpacePressed = true;
        }
    #else
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSpacePressed = true;
        }
    #endif

        if (isSpacePressed)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                Debug.Log("⌨️ 스페이스바 입력 감지: 서버에 다음 패킷('next')을 요청합니다.");
                websocket.SendText("next"); 
            }
        }
    }

    private void ProcessInferenceResult(CSIInferenceData data)
    {
        Debug.Log($"[Scenario 1] 수신 데이터 - 현재 인원수: {data.person_count}명");

        // 이번 패킷에 존재하는 사람 이름을 추적하기 위한 집합
        HashSet<string> currentNames = new HashSet<string>();

        if (data.person_count > 0 && data.presence != null)
        {
            // 1. 구역(Section/Zone)별로 사람 분류
            Dictionary<string, List<PresenceItem>> itemsPerZone = new Dictionary<string, List<PresenceItem>>();
            foreach (PresenceItem item in data.presence)
            {
                if (string.IsNullOrEmpty(item.name)) continue;

                currentNames.Add(item.name);

                string zoneName = string.IsNullOrEmpty(item.section) ? "Zone 1" : item.section;
                if (!itemsPerZone.ContainsKey(zoneName))
                {
                    itemsPerZone[zoneName] = new List<PresenceItem>();
                }
                itemsPerZone[zoneName].Add(item);
            }

            // 2. 구역별 사람 위치 계산 및 스폰/이동 처리
            foreach (var kvp in itemsPerZone)
            {
                string zoneName = kvp.Key;
                List<PresenceItem> itemsInZone = kvp.Value;
                int count = itemsInZone.Count;

                // 해당 구역의 매핑 정보 검색
                ZoneMapping mapping = zoneMappings.Find(m => m.zoneName.Equals(zoneName, StringComparison.OrdinalIgnoreCase));
                
                Vector3[] targetPositions = new Vector3[count];

                if (mapping != null && mapping.targetSection != null)
                {
                    // 기존 아키텍처: Section의 GetSpawnPositions를 활용하여 중앙정렬 정렬값 획득
                    targetPositions = mapping.targetSection.GetSpawnPositions(count);
                }
                else
                {
                    // Section이 없는 경우: customPosition을 기준으로 정렬
                    Vector3 basePos = (mapping != null) ? mapping.customPosition : Vector3.zero;
                    for (int i = 0; i < count; i++)
                    {
                        // 간단한 정렬 오프셋 적용 (서로 겹치지 않게 가로로 0.6f 간격 배치)
                        targetPositions[i] = basePos + new Vector3((i - (count - 1) / 2f) * 0.6f, 0f, 0f);
                    }
                }

                // 구역 내 사람들 스폰 및 배치
                for (int i = 0; i < count; i++)
                {
                    PresenceItem person = itemsInZone[i];
                    Vector3 spawnPos = targetPositions[i];
                    Color personColor = GetColorByName(person.name);

                    if (activeHumans.ContainsKey(person.name))
                    {
                        // 이미 스폰된 인물이면 위치 및 색상 업데이트
                        GameObject existingObj = activeHumans[person.name];
                        if (existingObj != null)
                        {
                            existingObj.transform.position = spawnPos;
                            
                            Human humanComp = existingObj.GetComponent<Human>();
                            if (humanComp != null)
                            {
                                humanComp.SetColor(personColor);
                            }
                        }
                    }
                    else
                    {
                        // 새로운 인물이면 스폰 후 색상 설정
                        GameObject newHuman = humanSpawnSystem.SpawnHuman(spawnPos, Quaternion.identity);
                        if (newHuman != null)
                        {
                            Human humanComp = newHuman.GetComponent<Human>();
                            if (humanComp != null)
                            {
                                humanComp.SetColor(personColor);
                            }
                            activeHumans[person.name] = newHuman;
                        }
                    }
                }
            }
        }

        // 3. 이번 수신 데이터에 없는(이전 프레임에 있었으나 퇴장한) 인물 despawn 처리
        List<string> toRemove = new List<string>();
        foreach (var key in activeHumans.Keys)
        {
            if (!currentNames.Contains(key))
            {
                toRemove.Add(key);
            }
        }

        foreach (string nameToRemove in toRemove)
        {
            GameObject objToDestroy = activeHumans[nameToRemove];
            if (objToDestroy != null)
            {
                humanSpawnSystem.DespawnHuman(objToDestroy);
            }
            activeHumans.Remove(nameToRemove);
        }
    }

    private Color GetColorByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return Color.white;

        string normalized = name.ToLower().Trim();

        // 한별: 다양한 영문 표기(hanbyul, hanbyeol) 및 포함 단어 대응
        if (normalized.Contains("한별") || normalized.Contains("hanbyul") || normalized.Contains("hanbyeol"))
        {
            return Color.red; // 한별 -> 빨강
        }
        // 민경: 다양한 영문 표기(minkyung, minkyoung, minkyeong) 대응
        else if (normalized.Contains("민경") || normalized.Contains("minkyung") || normalized.Contains("minkyoung") || normalized.Contains("minkyeong"))
        {
            return Color.yellow; // 민경 -> 노랑
        }
        // 민현: 다양한 영문 표기(minhyeon, minhyun) 대응
        else if (normalized.Contains("민현") || normalized.Contains("minhyeon") || normalized.Contains("minhyun"))
        {
            return Color.green; // 민현 -> 초록
        }
        // 조교님: 다양한 지칭 대응
        else if (normalized.Contains("조교") || normalized.Contains("ta") || normalized.Contains("assistant") || normalized.Contains("instructor"))
        {
            return Color.blue; // 조교님 -> 파랑
        }

        return Color.white; // 기본값 흰색
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
