using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
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

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectQueue);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        Queue<GameObject> poolQueue = poolDictionary[tag];

        // 풀이 비어있을 경우, 동적으로 확장합니다.
        if (poolQueue.Count == 0)
        {
            // 해당 태그를 가진 Pool 설정을 찾습니다.
            Pool p = pools.Find(pool => pool.tag == tag);
            if (p != null)
            {
                // 풀이 비어있으므로 새 오브젝트를 생성하여 확장합니다.
                Debug.LogWarning($"Pool with tag '{tag}' is empty. Expanding pool.");
                p.size++; // 에디터에서 추적할 수 있도록 풀 크기를 증가시킵니다.
                
                GameObject newObj = Instantiate(p.prefab);
                // 새롭게 생성된 오브젝트도 풀링 시스템의 관리를 받게 됩니다.
                // 이 오브젝트가 ReturnToPool을 통해 반환되면, 큐에 추가되어 풀의 전체 크기가 늘어납니다.
                newObj.SetActive(true);
                newObj.transform.position = position;
                newObj.transform.rotation = rotation;
                return newObj;
            }
            
            // Pool 설정을 찾지 못한 경우
            Debug.LogError($"Pool with tag '{tag}' is empty and its prefab could not be found to expand.");
            return null;
        }

        GameObject objectToSpawn = poolQueue.Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            Destroy(objectToReturn); // Or just disable it
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
