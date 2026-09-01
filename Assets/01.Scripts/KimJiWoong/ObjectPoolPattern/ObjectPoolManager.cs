using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [System.Serializable]
    public struct CanvasPoolItem
    {
        public string poolName;
        public GameObject prefab;
        public Transform targetCanvas;
        public int poolSize;
    }


    [System.Serializable]
    public struct ObjectPoolItem
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize;
    }

    //기본 오브젝트
    [SerializeField] private List<ObjectPoolItem> objList = new List<ObjectPoolItem>();
    //캔버스 전용 오브젝트 풀
    [SerializeField] private List<CanvasPoolItem> canvasPools = new List<CanvasPoolItem>();

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();
    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();



    void Start()
    {
        // 일반 오브젝트 풀 세팅
        foreach (var item in objList)
        {
            // 빈 문자열이거나 프리팹이 비어있으면 패스
            if (string.IsNullOrEmpty(item.poolName) || item.prefab == null) continue;

            prefabDict[item.poolName] = item.prefab;
            pools[item.poolName] = new Queue<GameObject>();

            GameObject parentPool = new GameObject($"{item.poolName}_Pool");
            parentPool.transform.SetParent(this.transform);

            SetupPool(item.poolName, item.prefab, parentPool.transform, item.poolSize);
        }

        // 캔버스 전용 오브젝트 풀 세팅 (작성하신 원본 로직 완벽 복구)
        foreach (var item in canvasPools)
        {
            if (string.IsNullOrEmpty(item.poolName) || item.prefab == null) continue;

            prefabDict[item.poolName] = item.prefab;
            pools[item.poolName] = new Queue<GameObject>();

            GameObject parentPool = new GameObject($"{item.poolName}_Pool");
            // UI 객체가 깨지지 않도록 targetCanvas에 false로 붙임
            parentPool.transform.SetParent(item.targetCanvas, false);

            SetupPool(item.poolName, item.prefab, parentPool.transform, item.poolSize);
        }
    }

    private void SetupPool(string poolName, GameObject prefab, Transform parent, int size)
    {
        poolParents[poolName] = parent;

        for (int i = 0; i < size; i++)
        {
            GameObject go = Instantiate(prefab, parent);
            go.name = poolName;
            go.SetActive(false);
            pools[poolName].Enqueue(go);
        }
    }

    public GameObject GetObject(string poolName)
    {
        if (!pools.ContainsKey(poolName))
        {
            return null;
        }

        if (pools[poolName].Count > 0)
        {
            GameObject go = pools[poolName].Dequeue();
            go.SetActive(true);
            return go;
        }
        else
        {
            GameObject prefab = GetPrefabFromList(poolName);

            if (prefab == null) return null;

            GameObject go = Instantiate(prefab, poolParents[poolName]);
            go.name = poolName;
            go.SetActive(true);
            return go;
        }
    }

    private GameObject GetPrefabFromList(string poolName)
    {
        if (prefabDict.TryGetValue(poolName, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }

    public void ReturnObject(string poolName, GameObject go)
    {
        if (!pools.ContainsKey(poolName))
        {
            Destroy(go);
            return;
        }
        go.SetActive(false);
        go.transform.SetParent(poolParents[poolName]);
        pools[poolName].Enqueue(go);
    }

}