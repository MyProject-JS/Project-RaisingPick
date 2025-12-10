using UnityEngine;
using System.Collections;

public class WarningEffect : MonoBehaviour
{
    [Tooltip("The tag used for pooling this warning effect.")]
    [SerializeField] private string poolTag = "WarningEffect";

    [Tooltip("How long the warning is visible before it disappears.")]
    [SerializeField] private float duration = 0.5f;
    
    private void OnEnable()
    {
        // 오브젝트가 활성화될 때, 지정된 시간 후에 자동으로 풀에 반환하는 코루틴을 시작합니다.
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        // 지정된 시간만큼 대기합니다.
        yield return new WaitForSeconds(duration);

        // 시간이 지난 후, ObjectPooler를 통해 자신을 풀에 반환합니다.
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            // 만약 ObjectPooler가 없다면, 그냥 파괴합니다.
            Destroy(gameObject);
        }
    }
}
