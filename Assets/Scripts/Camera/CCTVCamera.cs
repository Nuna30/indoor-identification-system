using UnityEngine;
using UnityEngine.InputSystem;

// 이 스크립트가 컴포넌트로 추가될 때, 유니티의 Camera 컴포넌트도 자동으로 함께 추가되도록 강제합니다.
[RequireComponent(typeof(Camera))]
public class CCTVCamera : MonoBehaviour
{
    [Header("회전 제한 설정")]
    [SerializeField] private float yawLimit = 60f;
    [SerializeField] private float pitchLimit = 45f;

    [Header("감도 설정")]
    [SerializeField] private float mouseSensitivity = 2f;

    private float yaw = 0f;
    private float pitch = 0f;
    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        // Mouse.current가 존재하면 델타 값을 가져옵니다.
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * 0.05f; // 감도 보정용 0.05 곱함

            yaw = Mathf.Clamp(yaw + mouseDelta.x * mouseSensitivity, -yawLimit, yawLimit);
            pitch = Mathf.Clamp(pitch - mouseDelta.y * mouseSensitivity, -pitchLimit, pitchLimit);
        }

        transform.localRotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);
    }
}
