using UnityEngine;
using UnityEngine.UI;

public class UI_OptionButton : MonoBehaviour
{
    [SerializeField] private Button optionButton;
    [SerializeField] private GameObject optionPopupPrefab; 

    private void Start()
    {
        if (optionButton != null)
        {
            optionButton.onClick.AddListener(OnClickOption);
        }
    }

    private void OnClickOption()
    {
        if (optionPopupPrefab != null && UIManager.Instance != null)
        {
            UIManager.Instance.ShowPopupGameObject(optionPopupPrefab);
        }
    }
}