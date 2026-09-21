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
        foreach (var popup in FindObjectsByType<UI_OptionPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (popup.gameObject.scene != gameObject.scene) continue;
            if (popup.gameObject.activeSelf) return;
            foreach (var manager in FindObjectsByType<UI_PopUpManager>(FindObjectsSortMode.None))
            {
                if (manager.gameObject.scene != gameObject.scene) continue;
                manager.ShowPopupGameObject(popup.gameObject);
                return;
            }
            popup.Open();
            return;
        }

        Debug.LogWarning("'OptionPopup'을 찾을 수 없습니다.");
    }
}
