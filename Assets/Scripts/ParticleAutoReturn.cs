using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAutoReturn : MonoBehaviour
{
    [Tooltip("이 파티클 시스템에 해당하는 오브젝트 풀 유형(Enum)")]
    [SerializeField] private PoolObjectType poolType = PoolObjectType.DestructionEffect;

    private ParticleSystem ps;

    public void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        StartCoroutine(ReturnAfterPlay());
    }

    private System.Collections.IEnumerator ReturnAfterPlay()
    {
        yield return new WaitForSeconds(ps.main.duration);

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
