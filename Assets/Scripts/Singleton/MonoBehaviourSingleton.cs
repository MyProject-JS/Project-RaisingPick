#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour, IInitializer where T : MonoBehaviour, IInitializer
{
    private static T _instance;


    public static T Instance
    {
        get
        {

            if (_instance == null)
            {
                // 인스턴스가 없을 때, 씬에서 찾기
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                if (_instance == null)
                {
                    // 씬에서도 못 찾았을 때, 새로 생성
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                    _instance.Init();
#if UNITY_EDITOR
                    EditorApplication.playModeStateChanged += Reset;
#endif
                }
            }

            return _instance;
        }
    }


#if UNITY_EDITOR
    public static void Reset(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            _instance = default;
            EditorApplication.playModeStateChanged -= Reset;
        }
    }
#endif

    
    public virtual void Init() { }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            Init();
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += Reset;
#endif
        }
        else if (_instance != this)
        {
            Debug.LogError(gameObject.name + " is destroyed");
            //valid 하지 않으면 삭제
            if (_instance.gameObject.scene.IsValid() == false)
                _instance = this as T;
            else
                Destroy(gameObject); // 중복 인스턴스가 생성된 경우 제거
        }
    }
}