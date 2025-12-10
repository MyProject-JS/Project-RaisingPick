using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 추가

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private int scoreValue = 10;

    [Header("Pooling")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string destructionEffectTag = "DestructionEffect";
    
    [Header("Off-screen Indicator")]
    [SerializeField] private string indicatorTag = "DistanceIndicator";
    
    private float moveSpeed;
    private Vector3 targetPosition = Vector3.zero;
    
    // Indicator-related variables
    private GameObject indicatorInstance;
    private TextMeshProUGUI indicatorText;
    private Camera mainCamera;
    private Transform canvasTransform;

    private void OnEnable()
    {
        moveSpeed = baseMoveSpeed;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            canvasTransform = FindObjectOfType<Canvas>()?.transform;
        }

        if (ObjectPooler.Instance != null && canvasTransform != null)
        {
            indicatorInstance = ObjectPooler.Instance.SpawnFromPool(indicatorTag, Vector3.zero, Quaternion.identity);
            if (indicatorInstance != null)
            {
                indicatorInstance.transform.SetParent(canvasTransform, false);
                indicatorText = indicatorInstance.GetComponentInChildren<TextMeshProUGUI>();
                
                // 깜빡임 현상을 막기 위해, 생성 시점에는 우선 비활성화합니다.
                indicatorInstance.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때, 사용하던 표시기를 풀에 반환합니다.
        if (ObjectPooler.Instance != null && indicatorInstance != null)
        {
            ObjectPooler.Instance.ReturnToPool(indicatorTag, indicatorInstance);
            indicatorInstance = null;
        }
    }

    void Update()
    {
        // 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 회전 (이동 방향을 바라보도록)
        Vector3 direction = targetPosition - transform.position;
        if (direction != Vector3.zero) // 0으로 나누는 것을 방지
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90); // +90도 오프셋은 스프라이트의 '위쪽'이 앞을 향하게 함
        }
    }

    void LateUpdate()
    {
        // 모든 이동 계산이 끝난 후, 표시기 위치를 계산하고 보여줍니다.
        HandleIndicator();
    }

    private void HandleIndicator()
    {
        if (indicatorInstance == null || mainCamera == null) return;
        
        // 표시기를 활성화합니다.
        indicatorInstance.SetActive(true);

        float distance = Vector3.Distance(transform.position, Vector3.zero);
        if (indicatorText != null)
        {
            indicatorText.text = distance.ToString("F0") + "M";
        }

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        Vector3 indicatorScreenPosition;
        
        bool isOnScreen = viewportPos.z > 0 && viewportPos.x > 0.05f && viewportPos.x < 0.95f && viewportPos.y > 0.05f && viewportPos.y < 0.95f;

        if (isOnScreen)
        {
            indicatorScreenPosition = mainCamera.WorldToScreenPoint(transform.position);
            indicatorScreenPosition.y += 30f;
        }
        else
        {
            Vector2 onScreenViewportPos = new Vector2(
                Mathf.Clamp(viewportPos.x, 0.05f, 0.95f),
                Mathf.Clamp(viewportPos.y, 0.05f, 0.95f)
            );
            indicatorScreenPosition = mainCamera.ViewportToScreenPoint(onScreenViewportPos);
        }
        
        indicatorInstance.transform.position = indicatorScreenPosition;
    }
    
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddScore(scoreValue);

            if (!string.IsNullOrEmpty(destructionEffectTag))
            {
                ObjectPooler.Instance.SpawnFromPool(destructionEffectTag, transform.position, Quaternion.identity);
            }
            
            ObjectPooler.Instance.ReturnToPool(enemyTag, gameObject);
        }
        else if (other.CompareTag("Core"))
        {
            GameManager.Instance.LoseLife();
            ObjectPooler.Instance.ReturnToPool(enemyTag, gameObject);
        }
    }
}
