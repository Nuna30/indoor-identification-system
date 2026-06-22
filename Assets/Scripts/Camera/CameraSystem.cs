using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CameraSystem : MonoBehaviour
{
    [Header("카메라 오브젝트 배열")]
    [SerializeField] private GameObject[] cctvCameras;

    private int currentCameraIndex = 0;

    private void Start()
    {
        if (cctvCameras == null || cctvCameras.Length == 0)
        {
            Debug.LogWarning("CameraSystem: 등록된 카메라 오브젝트가 없습니다.");
            return;
        }

        // 모든 카메라 상태 초기화 (첫 번째 카메라 오브젝트만 활성화)
        SelectCamera(0);
    }

    private void Update()
    {
        if (cctvCameras == null || cctvCameras.Length == 0) return;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // 숫자 키 1부터 9까지 체크하여 카메라 전환
            for (int i = 0; i < cctvCameras.Length && i < 9; i++)
            {
                Key targetKey = (Key)((int)Key.Digit1 + i);
                if (keyboard[targetKey].wasPressedThisFrame)
                {
                    SelectCamera(i);
                    break;
                }
            }
        }
#else
        // 레거시 Input Manager 사용
        for (int i = 0; i < cctvCameras.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectCamera(i);
                break;
            }
        }
#endif
    }

    public void SelectCamera(int index)
    {
        if (index < 0 || index >= cctvCameras.Length) return;

        currentCameraIndex = index;

        for (int i = 0; i < cctvCameras.Length; i++)
        {
            if (cctvCameras[i] == null) continue;

            // 해당 인덱스의 카메라 오브젝트만 켜고 나머지는 모두 끕니다.
            cctvCameras[i].SetActive(i == index);
        }
    }
}
