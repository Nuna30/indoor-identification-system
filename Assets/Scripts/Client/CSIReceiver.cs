using UnityEngine;
using System;
using NativeWebSocket;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 파이썬의 존재 리스트 내부 딕셔너리와 매핑 [{"name": "Minkyung", "section": "Zone 3"}]
[Serializable]
public class PresenceItem
{
    public string name;
    public string section;
}

// 전체 JSON 구조와 매핑
[Serializable]
public class CSIInferenceData
{
    public int person_count;
    public List<PresenceItem> presence;
}

public class CSIReceiver : MonoBehaviour
{
    private WebSocket websocket;

    async void Start()
    {
        // 1. 파이썬 FastAPI 웹소켓 주소 설정
        websocket = new WebSocket("ws://127.0.0.1:8000/ws/csi");

        // 2. 웹소켓 연결 이벤트 정의
        websocket.OnOpen += () =>
        {
            Debug.Log("📡 파이썬 CSI 추론 서버와 연결되었습니다!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"⚠️ 웹소켓 에러 발생: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("❌ 서버와의 연결이 해제되었습니다.");
        };

        // 3. [핵심] 서버로부터 실시간 JSON 메시지를 받았을 때 처리
        websocket.OnMessage += (bytes) =>
        {
            // 바이트 데이터를 문자열(JSON)로 변환
            string jsonText = System.Text.Encoding.UTF8.GetString(bytes);
            
            try
            {
                // C# 객체로 역직렬화(Parsing)
                CSIInferenceData data = JsonUtility.FromJson<CSIInferenceData>(jsonText);
                
                // 유니티 콘솔창 및 화면 처리를 위한 함수 호출
                ProcessInferenceResult(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 파싱 실패: {ex.Message}");
            }
        };

        // 4. 비동기 서버 연결 시작
        await websocket.Connect();
    }

    void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        // 웹소켓 메시지 큐 업데이트 (기존 코드)
        websocket.DispatchMessageQueue();
    #endif

        // [핵심 추가] 유니티 안에서 스페이스바를 누르면 파이썬 서버로 다음 패킷 요청 전달!
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

    // 4. 수신된 데이터를 바탕으로 실시간 UI/오브젝트 제어 로직 구현 영역
    private void ProcessInferenceResult(CSIInferenceData data)
    {
        Debug.Log($"[실시간 수신] 현재 인원수: {data.person_count}명");

        if (data.person_count == 0)
        {
            Debug.Log("현재 구역에 아무도 없습니다 (Empty). 모든 마커를 숨깁니다.");
            // TODO: 유니티 화면에 떠있는 모든 사람 마커 비활성화 로직
            return;
        }

        // 인물별 위치(Zone) 루프 돌며 마커 이동 처리
        foreach (PresenceItem item in data.presence)
        {
            Debug.Log($"🎯 인물명: {item.name} | 위치 구역: {item.section}");
            
            // TODO: item.name(Minkyung, TA 등)에 해당하는 캐릭터 마커 오브젝트를 찾아서
            // item.section(Zone 1, Zone 2 등)에 해당하는 위치 좌표로 Transform.position 변경하는 로직 추가
        }
    }

    private async void OnApplicationQuit()
    {
        // 유니티 앱이 종료될 때 안전하게 소켓 닫기
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}