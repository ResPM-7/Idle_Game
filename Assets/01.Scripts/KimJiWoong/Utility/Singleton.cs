using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static bool _applicationIsQuitting = false;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();

                    var singleton = _instance as Singleton<T>;

                    if (singleton != null && singleton.isDontDestroy)
                    {
                        DontDestroyOnLoad(obj);
                    }
                }
            }
            else
            {
                var singleton = _instance as Singleton<T>;

                if (singleton != null && singleton.isDontDestroy)
                {
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }

    [SerializeField] protected bool isDontDestroy = false;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (isDontDestroy)
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}
