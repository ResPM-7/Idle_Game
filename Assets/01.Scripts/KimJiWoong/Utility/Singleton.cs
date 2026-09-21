using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static bool _applicationIsQuitting = false;

    public static T instance
    {
        get
        {
            if (_applicationIsQuitting)
                return null;

            if (_instance == null)
            {
                // 씬에 배치된 인스턴스만 찾습니다.
                // 존재하지 않는다고 빈 매니저를 생성하지 않습니다.
                _instance = FindFirstObjectByType<T>();
            }

            return _instance;
        }
    }

    [SerializeField] protected bool isDontDestroy = false;

    protected virtual void Awake()
    {
        _applicationIsQuitting = false;

        if (_instance == null)
        {
            _instance = this as T;

            if (isDontDestroy)
                DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        // 씬 이동으로 파괴될 때는 인스턴스만 비웁니다.
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        // 실제 애플리케이션 종료 때만 true로 변경합니다.
        _applicationIsQuitting = true;
    }
}
