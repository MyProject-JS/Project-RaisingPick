using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모든 적의 공통 행동을 정의하는 MonoBehaviour입니다.
/// 실제 능력치는 EnemyData ScriptableObject를 통해 주입받습니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))] // SpriteRenderer가 필수 컴포넌트임을 명시
public class Enemy : MonoBehaviour
{
    // --- Public Properties ---
    public EnemyData Data { get; private set; }

    // --- Private State ---
    private float currentMoveSpeed;
    private float maxColorDistance; // 스포너로부터 받아오는 최대 거리 (생성 반경)
    private Vector3 targetPosition = Vector3.zero;
    
    // --- Cached Components & References ---
    private GameObject indicatorInstance;
    private TextMeshProUGUI indicatorText;
    private Image indicatorImage;
    private Camera mainCamera;
    private Transform canvasTransform;
    private SpriteRenderer spriteRenderer; // 적의 외형을 바꿀 SpriteRenderer

    private void Awake()
    {
        // 프리팹 자체에 있는 컴포넌트는 Awake에서 캐싱하는 것이 효율적입니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 스포너에 의해 호출되어 적의 데이터를 설정하고 동적 상태를 초기화합니다.
    /// </summary>
    public void Initialize(EnemyData data, float dynamicSpeed, float spawnRadius)
    {
        this.Data = data;
        this.currentMoveSpeed = dynamicSpeed;
        this.maxColorDistance = spawnRadius;

        // 데이터에 따라 스프라이트를 설정합니다.
        if (spriteRenderer != null && Data.enemySprite != null)
        {
            spriteRenderer.sprite = Data.enemySprite;
        }
    }

    private void OnEnable()
    {
        // 캐싱되지 않은 경우에만 컴포넌트와 오브젝트를 찾습니다.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            canvasTransform = FindFirstObjectByType<Canvas>()?.transform;
        }
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때, 사용하던 표시기를 풀에 반환합니다.
        if (ObjectPooler.Instance != null && indicatorInstance != null)
        {
            if(Data != null)
            {
                ObjectPooler.Instance.ReturnToPool(Data.indicatorType, indicatorInstance);
            }
            indicatorInstance = null;
        }
        
        // Data 참조를 리셋하여 풀에서 재사용될 때를 대비합니다.
        Data = null;
    }

    void Update()
    {
        // Data가 없으면 움직이지 않습니다.
        if (Data == null) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentMoveSpeed * Time.deltaTime);

        Vector3 direction = targetPosition - transform.position;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        }
    }

    void LateUpdate()
    {
        // Data가 아직 할당되지 않았다면 아무것도 하지 않습니다.
        if (Data == null) return;
        
        // 표시기 인스턴스가 없으면 생성합니다.
        if (indicatorInstance == null)
        {
            CreateIndicator();
        }
        
        HandleIndicator();
    }

    /// <summary>
    /// 표시기 오브젝트를 풀에서 가져와 설정합니다.
    /// </summary>
    private void CreateIndicator()
    {
        if (ObjectPooler.Instance != null && canvasTransform != null)
        {
            // indicatorType이 None이 아닌 경우에만 표시기를 생성합니다.
            if (Data.indicatorType != PoolObjectType.None)
            {
                indicatorInstance = ObjectPooler.Instance.SpawnFromPool(Data.indicatorType, Vector3.zero, Quaternion.identity);
                if (indicatorInstance != null)
                {
                    indicatorInstance.transform.SetParent(canvasTransform, false);
                    indicatorText = indicatorInstance.GetComponentInChildren<TextMeshProUGUI>();
                    indicatorImage = indicatorInstance.GetComponentInChildren<Image>();
                    indicatorInstance.SetActive(false);
                }
            }
        }
    }

    private void HandleIndicator()
    {
        if (indicatorInstance == null || mainCamera == null) return;
        
        indicatorInstance.SetActive(true);

        float distance = Vector3.Distance(transform.position, Vector3.zero);
        
        if (indicatorText != null)
        {
            indicatorText.text = distance.ToString("F0") + "M";
        }

        if (indicatorImage != null)
        {
            float t = Mathf.InverseLerp(Data.minColorDistance, maxColorDistance, distance);
            indicatorImage.color = Color.Lerp(Data.nearColor, Data.farColor, t);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Data == null) return;

        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddScore(Data.scoreValue);

            // 파괴 이펙트 타입이 설정되어 있을 경우에만 스폰합니다.
            if (Data.destructionEffectType != PoolObjectType.None)
            {
                ObjectPooler.Instance.SpawnFromPool(Data.destructionEffectType, transform.position, Quaternion.identity);
            }
            
            ObjectPooler.Instance.ReturnToPool(Data.poolType, gameObject);
        }
        else if (other.CompareTag("Core"))
        {
            GameManager.Instance.LoseLife();
            ObjectPooler.Instance.ReturnToPool(Data.poolType, gameObject);
        }
    }
}
