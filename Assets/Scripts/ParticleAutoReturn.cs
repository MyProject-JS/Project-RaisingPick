using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAutoReturn : MonoBehaviour
{
    [Tooltip("The tag used for pooling this particle system.")]
    [SerializeField] private string poolTag = "DestructionEffect";

    private ParticleSystem ps;

    public void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // Start a coroutine to return the particle system to the pool after it has finished
        StartCoroutine(ReturnAfterPlay());
    }

    private System.Collections.IEnumerator ReturnAfterPlay()
    {
        // Wait for the duration of the particle system
        yield return new WaitForSeconds(ps.main.duration);

        // Return the object to the pool
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            // If the pooler is gone, just destroy the object
            Destroy(gameObject);
        }
    }
}
