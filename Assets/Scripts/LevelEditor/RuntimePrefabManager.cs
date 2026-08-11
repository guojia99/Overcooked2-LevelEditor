using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public static class RuntimePrefabManager
{
    static List<GameObject> runtimePrefabsClearOnRestart = new List<GameObject>();
    static List<GameObject> runtimePrefabsClearOnQuit = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScreen")
        {
            ClearAllRuntimePrefabs(true);
        }
    }

    public static GameObject CloneAsInactivePrefab(GameObject original, bool clearOnRestart = true)
    {
        original.SetActive(false);
        GameObject prefab = GameObject.Instantiate(original, Vector3.up * 1000, Quaternion.identity);
        SetHideFlagsRecursive(prefab, HideFlags.HideAndDontSave);
        original.SetActive(true);
        if (clearOnRestart)
            runtimePrefabsClearOnRestart.Add(prefab);
        else 
            runtimePrefabsClearOnQuit.Add(prefab);
        return prefab;
    }

    public static void SetHideFlagsRecursive(GameObject gameObject, HideFlags hideFlags)
    {
        Transform[] allTransforms = gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            t.gameObject.hideFlags = hideFlags;
        }
    }

    public static void ClearAllRuntimePrefabs(bool isQuit)
    {
        foreach (var prefab in runtimePrefabsClearOnRestart)
            Object.DestroyImmediate(prefab);
        runtimePrefabsClearOnRestart.Clear();
        if (isQuit)
        {
            foreach (var prefab in runtimePrefabsClearOnQuit)
                Object.DestroyImmediate(prefab);
            runtimePrefabsClearOnQuit.Clear();
        }
    }
}