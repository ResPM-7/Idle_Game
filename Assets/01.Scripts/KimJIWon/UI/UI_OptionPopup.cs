using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_OptionPopup : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backToTitleButton;
    [SerializeField] private UI_SceneNavigation navigation;
    private bool pausedByThisPopup;
    private float previousTimeScale;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        backToTitleButton.onClick.AddListener(navigation.GoToTitleScene);
    }

    private void OnEnable()
    {
        bool isMain = gameObject.scene.path == UI_SceneNavigation.MainScenePath;
        backToTitleButton.gameObject.SetActive(isMain);
        if (isMain)
        {
            previousTimeScale = Time.timeScale;
            pausedByThisPopup = true;
            Time.timeScale = 0f;
        }
    }

    public void Open()
    {
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        // MainScene opens this popup through its existing stack manager.
        if (gameObject.scene.path == UI_SceneNavigation.MainScenePath)
        {
            var manager = FindFirstObjectByType<UI_PopUpManager>();
            if (manager != null) manager.CloseTopPopup();
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // MainScene's popup manager already handles Escape.
        if (gameObject.scene.path != UI_SceneNavigation.MainScenePath &&
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void OnDisable()
    {
        if (!pausedByThisPopup) return;
        Time.timeScale = previousTimeScale;
        pausedByThisPopup = false;
    }
}
