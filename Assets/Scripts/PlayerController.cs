using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float rotationSpeed = 100f; // 회전 속도 (도/초)
    [Tooltip("플레이어가 회전하는 원의 반지름입니다.")]
    [SerializeField] private float radius = 2f; // 중앙과의 거리 (반지름)

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please tag a camera as 'MainCamera'.");
        }
        
        // 게임 시작 시 플레이어 상태를 리셋
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        // 플레이어 위치를 초기값(오른쪽)으로 리셋
        transform.position = new Vector3(radius, 0, 0);
        // 플레이어가 바깥쪽을 바라보도록 초기 회전값 설정 (-90도)
        transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    void Update()
    {
        // 마우스 왼쪽 버튼을 누르고 있거나 화면을 터치하고 있을 때
        if (Input.GetMouseButton(0))
        {
            // 마우스 위치를 월드 좌표로 변환
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            // 중앙(0,0)에서 마우스 위치를 향하는 목표 각도 계산
            float targetAngle = Mathf.Atan2(mouseWorldPos.y, mouseWorldPos.x) * Mathf.Rad2Deg;

            // 현재 플레이어의 각도 계산
            float currentAngle = Mathf.Atan2(transform.position.y, transform.position.x) * Mathf.Rad2Deg;

            // 현재 각도에서 목표 각도로 부드럽게 이동
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

            // 계산된 새 각도를 사용하여 원 위의 새 위치를 설정
            transform.position = new Vector3(
                radius * Mathf.Cos(newAngle * Mathf.Deg2Rad),
                radius * Mathf.Sin(newAngle * Mathf.Deg2Rad),
                0
            );

            // 플레이어가 바깥쪽을 바라보도록 회전 (선택 사항이지만 자연스러움을 더함)
            transform.rotation = Quaternion.Euler(0, 0, newAngle - 90);
        }
    }
}
