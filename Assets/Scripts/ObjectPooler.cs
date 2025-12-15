using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // A container to hold both the queue of inactive objects and a list of all created objects for a pool
    private class PoolContainer
    {
        public Queue<GameObject> InactiveObjects { get; } = new Queue<GameObject>();
        public List<GameObject> AllCreatedObjects { get; } = new List<GameObject>();
    }

    [System.Serializable]
    public class Pool
    {
        [Tooltip("오브젝트의 유형 (Enum)")]
        public PoolObjectType type;
        [Tooltip("해당 유형으로 스폰될 프리팹")]
        public GameObject prefab;
        [Tooltip("초기 풀 크기")]
        public int size;
    }

    #region Singleton
    public static ObjectPooler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [Tooltip("에디터에서 설정할 오브젝트 풀 목록")]
    public List<Pool> pools;
    
    // The dictionary now holds the more complex PoolContainer
    private Dictionary<PoolObjectType, PoolContainer> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<PoolObjectType, PoolContainer>();

        foreach (Pool pool in pools)
        {
            var poolContainer = new PoolContainer();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                poolContainer.InactiveObjects.Enqueue(obj);
                poolContainer.AllCreatedObjects.Add(obj);
            }
            poolDictionary.Add(pool.type, poolContainer);
        }
    }

    public GameObject SpawnFromPool(PoolObjectType type, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.TryGetValue(type, out var poolContainer))
        {
            Debug.LogWarning("Pool with type " + type + " doesn't exist.");
            return null;
        }

        // If the pool is empty, expand it
        if (poolContainer.InactiveObjects.Count == 0)
        {
            Pool p = pools.Find(pool => pool.type == type);
            if (p != null)
            {
                Debug.LogWarning($"Pool with type '{type}' is empty. Expanding pool.");
                p.size++;
                
                GameObject newObj = Instantiate(p.prefab);
                poolContainer.AllCreatedObjects.Add(newObj); // Track the new object
                
                newObj.SetActive(true);
                newObj.transform.position = position;
                newObj.transform.rotation = rotation;
                return newObj;
            }
            
            Debug.LogError($"Pool with type '{type}' is empty and its prefab could not be found to expand.");
            return null;
        }

        GameObject objectToSpawn = poolContainer.InactiveObjects.Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(PoolObjectType type, GameObject objectToReturn)
    {
        if (!poolDictionary.TryGetValue(type, out var poolContainer))
        {
            Debug.LogWarning("Pool with type " + type + " doesn't exist. Destroying object.");
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);
        poolContainer.InactiveObjects.Enqueue(objectToReturn);
    }

    /// <summary>
    /// 모든 풀에서 현재 활성화된 모든 오브젝트를 비활성화하고 풀에 반환합니다.
    /// </summary>
    public void ReturnAllToPool()
    {
        foreach(var pair in poolDictionary)
        {
            var poolType = pair.Key;
            var container = pair.Value;
            
            // This is more efficient than FindGameObjectsWithTag
            foreach(var obj in container.AllCreatedObjects)
            {
                if(obj != null && obj.activeInHierarchy)
                {
                    ReturnToPool(poolType, obj);
                }
            }
        }
    }
}
