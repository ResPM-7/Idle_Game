using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private UI_PopUpManager popUpManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }
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
}
