using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    [Serializable]
    public struct CanvasPoolItem
    {
        public string poolName;
        public GameObject prefab;
        public Transform targetCanvas;
        public int poolSize;
    }

    [Serializable]
    public struct ObjectPoolItem
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize;
    }

    // 기존 인스펙터 등록이 유지되도록 필드 이름을 변경하지 않습니다.
    [SerializeField] public List<ObjectPoolItem> objList = new List<ObjectPoolItem>();
    [SerializeField] public List<CanvasPoolItem> canvasPools = new List<CanvasPoolItem>();

    private enum MemberState { Stored, Preparing, Active, Returning }
    private sealed class Pool
    {
        public string Name;
        public GameObject Prefab;
        public Transform Parent;
        public readonly Queue<GameObject> Available = new Queue<GameObject>();
    }
    // 객체마다 한 번만 등록합니다. 실행 중 컴포넌트를 추가하거나 기존 프리팹을 수정할 필요가 없습니다.
    private sealed class Member
    {
        public Pool Pool;
        public MemberState State;
        public uint Version;
        public IPoolable[] Callbacks;
    }
    private readonly Dictionary<string, Pool> pools = new Dictionary<string, Pool>();
    private readonly Dictionary<GameObject, Member> members = new Dictionary<GameObject, Member>();
    private Transform inactiveRoot;
    private bool initialized;

    private void Start() => Initialize();

    /// <summary>등록 목록으로 풀을 한 번만 준비합니다. 생성 요청 시에도 자동으로 호출됩니다.</summary>
    public void Initialize()
    {
        if (initialized) return;
        initialized = true;
        var root = new GameObject("Pool_InactiveStorage");
        root.SetActive(false);
        root.transform.SetParent(transform, false);
        inactiveRoot = root.transform;
        foreach (var item in objList)
            Register(item.poolName, item.prefab, transform, item.poolSize);
        foreach (var item in canvasPools)
        {
            if (item.targetCanvas == null)
            {
                Debug.LogError($"[오브젝트 풀] '{item.poolName}'의 대상 캔버스가 연결되지 않았습니다.", this);
                continue;
            }
            Register(item.poolName, item.prefab, item.targetCanvas, item.poolSize);
        }
    }

    private void Register(string key, GameObject prefab, Transform parent, int size)
    {
        if (string.IsNullOrWhiteSpace(key) || prefab == null)
        {
            Debug.LogError($"[오브젝트 풀] '{key}'의 등록 정보가 잘못되었습니다. 풀 이름과 프리팹을 확인해 주세요.", this);
            return;
        }
        if (pools.ContainsKey(key))
        {
            Debug.LogError($"[오브젝트 풀] '{key}' 이름이 중복되었습니다. 처음 등록한 항목만 사용합니다.", this);
            return;
        }
        var root = new GameObject(key + "_Pool");
        root.transform.SetParent(parent, false);
        var pool = new Pool { Name = key, Prefab = prefab, Parent = root.transform };
        pools.Add(key, pool);
        for (int i = 0; i < Mathf.Max(0, size); i++)
            pool.Available.Enqueue(CreateMember(pool));
    }

    private GameObject CreateMember(Pool pool)
    {
        // 비활성 부모 아래에서 미리 생성하여 OnEnable과 이펙트가 준비 중 실행되지 않도록 합니다.
        GameObject obj = Instantiate(pool.Prefab, inactiveRoot, false);
        obj.name = pool.Name;
        obj.SetActive(false);
        obj.transform.SetParent(pool.Parent, false);
        var callbacks = new List<IPoolable>();
        foreach (var behaviour in obj.GetComponentsInChildren<MonoBehaviour>(true))
            if (behaviour is IPoolable callback) callbacks.Add(callback);
        members.Add(obj, new Member { Pool = pool, Callbacks = callbacks.ToArray() });
        return obj;
    }

    // 기존 호출을 위한 호환 함수입니다. 데이터 설정이 필요하면 초기화 콜백을 받는 Spawn을 사용하세요.
    public GameObject GetObject(string poolName) => Spawn(poolName);

    /// <summary>
    /// 비활성 객체 확보 → prepare에서 데이터 설정 → 활성화 → OnSpawned 순서로 실행합니다.
    /// 준비 함수에서는 직접 활성화하지 마세요. 초기화 실패 또는 취소 시 null을 반환합니다.
    /// 비활성 객체는 Awake가 아직 실행되지 않았을 수 있으므로 준비에 필요한 참조를 직접 확인하세요.
    /// </summary>
    public GameObject Spawn(string poolName, Action<GameObject> prepare = null)
    {
        Initialize();
        if (string.IsNullOrEmpty(poolName) || !pools.TryGetValue(poolName, out var pool))
        {
            Debug.LogWarning($"[오브젝트 풀] '{poolName}'이 등록되지 않았습니다. 인스펙터의 풀 이름을 확인해 주세요.", this);
            return null;
        }
        if (pool.Parent == null)
        {
            Debug.LogError($"[오브젝트 풀] '{poolName}'의 보관 부모가 삭제되었습니다. 씬 또는 캔버스가 먼저 삭제되었는지 확인해 주세요.", this);
            return null;
        }
        GameObject obj = null;
        while (pool.Available.Count > 0 && obj == null)
        {
            obj = pool.Available.Dequeue();
            if (obj == null && !ReferenceEquals(obj, null)) members.Remove(obj);
        }
        if (obj == null) obj = CreateMember(pool);
        Member member = members[obj];
        // 콜백 안에서 반환 후 다시 꺼낸 객체를 이전 생성 요청이 건드리지 않도록 구분합니다.
        uint version = ++member.Version;
        member.State = MemberState.Preparing;
        try
        {
            prepare?.Invoke(obj);
            if (obj == null || member.Version != version || member.State != MemberState.Preparing) return null;
            member.State = MemberState.Active;
            obj.SetActive(true);
            if (obj == null || member.Version != version || member.State != MemberState.Active) return null;
            if (!obj.activeInHierarchy)
                throw new InvalidOperationException("[오브젝트 풀] 생성 위치의 부모가 비활성 상태입니다. 부모 오브젝트를 활성화해 주세요.");
            foreach (var callback in member.Callbacks)
            {
                if (obj == null || member.Version != version || member.State != MemberState.Active) return null;
                if (callback is MonoBehaviour behaviour && behaviour != null && behaviour.isActiveAndEnabled)
                    callback.OnSpawned();
            }
            return member.Version == version && member.State == MemberState.Active ? obj : null;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (member.Version == version) ReturnObject(obj);
            return null;
        }
    }

    public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
        => Spawn(poolName, obj => obj.transform.SetPositionAndRotation(position, rotation));

    /// <summary>이 매니저가 꺼내어 사용 중인 객체인지 확인합니다. 단순 활성 여부와는 다릅니다.</summary>
    public bool IsRented(GameObject obj)
        => obj != null && members.TryGetValue(obj, out var member) && member.State == MemberState.Active;

    // 기존 호출자가 전달한 이름보다 실제로 등록된 소속 풀을 우선합니다.
    public void ReturnObject(string poolName, GameObject obj)
    {
        if (obj != null && members.TryGetValue(obj, out var member) && member.Pool.Name != poolName)
            Debug.LogWarning($"[오브젝트 풀] 요청한 풀 '{poolName}'과 실제 소속이 다릅니다. 원래 풀 '{member.Pool.Name}'로 반환합니다.", obj);
        ReturnObject(obj);
    }

    /// <summary>사용 상태 정리 → 비활성화 → 원래 부모로 이동 → 보관 순서로 반환합니다.</summary>
    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;
        if (!members.TryGetValue(obj, out var member))
        {
            Debug.LogWarning($"[오브젝트 풀] '{obj.name}'은 이 매니저가 관리하는 객체가 아닙니다. 반환하거나 삭제하지 않습니다.", obj);
            return;
        }
        // 중복 반환과 반환 콜백 안에서의 재반환을 막아 큐에 한 번만 보관합니다.
        if (member.State == MemberState.Stored || member.State == MemberState.Returning) return;
        member.State = MemberState.Returning;
        foreach (var callback in member.Callbacks)
        {
            if (!(callback is MonoBehaviour behaviour) || behaviour == null) continue;
            try { callback.OnDespawned(); }
            catch (Exception exception) { Debug.LogException(exception, behaviour); }
        }
        if (obj == null) { members.Remove(obj); return; }
        obj.SetActive(false);
        if (member.Pool.Parent == null)
        {
            members.Remove(obj);
            Destroy(obj);
            return;
        }
        obj.transform.SetParent(member.Pool.Parent, false);
        member.State = MemberState.Stored;
        member.Pool.Available.Enqueue(obj);
    }
}
