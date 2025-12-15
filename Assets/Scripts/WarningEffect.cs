using UnityEngine;
using System.Collections;

public class WarningEffect : MonoBehaviour
{
    [Tooltip("이 경고 이펙트에 해당하는 오브젝트 풀 유형(Enum)")]
    [SerializeField] private PoolObjectType poolType = PoolObjectType.WarningEffect;

    [Tooltip("경고가 사라지기 전까지 표시되는 시간입니다.")]
    [SerializeField] private float duration = 0.5f;
    
    private void OnEnable()
    {
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(duration);

        if (ObjectPooler.Instance != null)
        {
            // 이제 string 대신 enum 타입을 사용하여 풀에 반환합니다.
            ObjectPooler.Instance.ReturnToPool(poolType, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
