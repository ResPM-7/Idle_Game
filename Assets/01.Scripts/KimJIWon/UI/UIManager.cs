using System.Collections.Generic;

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private UI_PopUpManager popUpManager;

    [Header("Canvases")]
    [SerializeField] private Transform overlayCanvasTransform;

    [Header("Prefabs")]
    [SerializeField] private GameObject damageTextPrefab;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //Canvas_Overlay 등록
            if (overlayCanvasTransform == null)
            {
                GameObject overlayObj = GameObject.Find("Canvas_Overlay");
                if (overlayObj != null)
                {
                    overlayCanvasTransform = overlayObj.transform;
                }
            }
            //UI_PopUpManager 등록
            if (popUpManager == null)
            {
                popUpManager = FindAnyObjectByType<UI_PopUpManager>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region PopUp
    public T ShowPopup<T>(T popupPrefab) where T : MonoBehaviour
    {
        if (popUpManager != null)
            return popUpManager.ShowPopup<T>(popupPrefab);

        Debug.LogWarning("UI_PopUpManager가 UIManager에 연결되지 않았습니다.");
        return null;
    }
    public void ShowPopupGameObject(GameObject popupObject)
    {
        if (popUpManager != null)
            popUpManager.ShowPopupGameObject(popupObject);
    }

    public void CloseTopPopup()
    {
        if (popUpManager != null)
            popUpManager.CloseTopPopup();
    }

    public void CloseAllPopups()
    {
        if (popUpManager != null)
            popUpManager.CloseAllPopups();
    }
    #endregion

    #region InGame UI / Floating Text
    public void ShowDamageText(float damage, Vector3 worldPos)
    {
        string poolKey = "DamageText";

        if (ObjectPoolManager.instance == null) return;

        EnsurePoolRegistered(poolKey, damageTextPrefab);

        GameObject textObj = ObjectPoolManager.instance.GetObject(poolKey); 
        if (textObj != null)
        {
            
            if (overlayCanvasTransform != null)
            {
                textObj.transform.SetParent(overlayCanvasTransform, false);
            }

            if (textObj.TryGetComponent<UI_DamageText>(out var damageText))
            {
                damageText.Setup(damage, worldPos, poolKey);
            }
        }
    }
    #endregion

    private void EnsurePoolRegistered(string poolKey, GameObject prefab)
    {
        var managerType = typeof(ObjectPoolManager);
        var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        var poolsField = managerType.GetField("pools", bindingFlags);
        if (poolsField == null) return;

        var pools = poolsField.GetValue(ObjectPoolManager.instance) as Dictionary<string, Queue<GameObject>>;
        if (pools != null && !pools.ContainsKey(poolKey))
        {
            // ObjectPoolManager 내부 딕셔너리 세팅을 UIManager가 보완
            var parentsField = managerType.GetField("poolParents", bindingFlags);
            var prefabsField = managerType.GetField("prefabDict", bindingFlags);

            var parents = parentsField?.GetValue(ObjectPoolManager.instance) as Dictionary<string, Transform>;
            var prefabs = prefabsField?.GetValue(ObjectPoolManager.instance) as Dictionary<string, GameObject>;

            pools[poolKey] = new Queue<GameObject>();

            GameObject parentPool = new GameObject($"{poolKey}_Pool");
            if (overlayCanvasTransform != null)
                parentPool.transform.SetParent(overlayCanvasTransform, false);

            if (parents != null) parents[poolKey] = parentPool.transform;
            if (prefabs != null) prefabs[poolKey] = prefab;
        }
    }
}
