using UnityEngine;
using UnityEngine.UI;

public class UI_OptionButton : MonoBehaviour
{
    private Button optionButton;

    private void Awake()
    {
        //컴포넌트 자동 탐색
        optionButton = GetComponent<Button>();

        if (optionButton != null)
        {
            optionButton.onClick.AddListener(OnClickOption);
        }
    }

    private void OnClickOption()
    {
        if (UIManager.Instance == null) return;

        Transform popupCanvas = GameObject.Find("Canvas_PopUp")?.transform;
        if (popupCanvas != null)
        {
            Transform optionPopupTrans = popupCanvas.Find("OptionPopup");
            if (optionPopupTrans != null)
            {
                UIManager.Instance.ShowPopupGameObject(optionPopupTrans.gameObject);
                return;
            }
        }

        Debug.LogWarning("'OptionPopup'을 찾을 수 없습니다.");
    }
}