using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IInitializer
{
    public void Init();
}
public class Singleton<T>: IInitializer where T: IInitializer, new() 
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
                #if UNITY_EDITOR
                EditorApplication.playModeStateChanged += Singleton<T>.Reset;
                #endif
                instance.Init();
            }

            return instance;
        }
    }
#if UNITY_EDITOR    
    public static void Reset(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            instance = default;
            EditorApplication.playModeStateChanged -= Singleton<T>.Reset;
        }
    }
#endif    
    public virtual void Init() { }
}
