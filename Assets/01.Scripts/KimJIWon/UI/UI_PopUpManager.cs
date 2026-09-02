using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_PopUpManager : MonoBehaviour
{
    [SerializeField] private Transform popupCanvasTransform;

    private Stack_PopupUI activePopupStack = new Stack_PopupUI();

    private void Awake()
    {
        
        if (popupCanvasTransform == null || !popupCanvasTransform.gameObject.scene.IsValid())
        {
            GameObject popupObj = GameObject.Find("Canvas_PopUp");
            if (popupObj != null)
            {
                popupCanvasTransform = popupObj.transform;
            }
            else
            {
                Debug.LogWarning("씬에서 Canvas_PopUp을 찾을 수 없습니다.");
            }
        }
    }

    public T ShowPopup<T>(T popupPrefab) where T : MonoBehaviour
    {
        if (popupPrefab == null) return null;

        // 팝업 생성
        T popupInstance = Instantiate(popupPrefab, popupCanvasTransform);
        popupInstance.gameObject.SetActive(true);

        // 스택에 등록
        activePopupStack.Push(popupInstance.gameObject);
       

        return popupInstance;
    }

    public void ShowPopupGameObject(GameObject popupObject)
    {
        if (popupObject == null) return;

        popupObject.transform.SetParent(popupCanvasTransform, false);
        popupObject.transform.SetAsLastSibling(); // 최상단으로 올림
        popupObject.SetActive(true);

        activePopupStack.Push(popupObject);
        
    }
    public void CloseTopPopup()
    {
        if (activePopupStack.Count > 0)
        {
            GameObject topPopup = activePopupStack.Pop();
            Destroy(topPopup);

        }
    }
    public void CloseAllPopups()
    {
        while (activePopupStack.Count > 0)
        {
            GameObject popup = activePopupStack.Pop();
            Destroy(popup);
        }
        
    }
    private void Update()
    {
        if (Keyboard.current != null)
        {
            
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseTopPopup();
            }
        }
    }
}
public class Stack_PopupUI : Stack<GameObject> { }
