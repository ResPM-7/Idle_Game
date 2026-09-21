using TMPro;
using UnityEngine;

public class UI_DisplaySettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown displayModeDropdown;

    private readonly Vector2Int[] resolutions = { new(1920, 1080),new(1600, 900),new(1280, 720) };

    private void Awake()
    {
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        displayModeDropdown.onValueChanged.AddListener(ChangeDisplayMode);
    }

    private void Start()
    {
        InitializeDropdowns();
    }

    private void InitializeDropdowns()
    {
        // 현재 해상도와 일치하는 항목 선택
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (Screen.width == resolutions[i].x &&
                Screen.height == resolutions[i].y)
            {
                resolutionDropdown.SetValueWithoutNotify(i);
                break;
            }
        }

        // 0: 전체화면, 1: 창 모드
        int modeIndex = Screen.fullScreenMode == FullScreenMode.Windowed ? 1 : 0;

        displayModeDropdown.SetValueWithoutNotify(modeIndex);
    }

    private void ChangeResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length)
            return;

        Vector2Int resolution = resolutions[index];

        Debug.Log($"[DisplaySettings] 해상도 요청: {resolution.x} x {resolution.y}");

        Screen.SetResolution(resolution.x,resolution.y,Screen.fullScreenMode);
    }

    private void ChangeDisplayMode(int index)
    {
        FullScreenMode mode = index == 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        Debug.Log($"[DisplaySettings] 화면 모드 요청: {mode}");

        Screen.SetResolution(Screen.width, Screen.height, mode);

        StartCoroutine(LogScreenState());
    }

    private System.Collections.IEnumerator LogScreenState()
    {
        // 한 프레임 대기
        yield return null;

        Debug.Log(
            $"[DisplaySettings] 현재 상태: " +
            $"{Screen.width} x {Screen.height}, " +
            $"모드={Screen.fullScreenMode}"
        );
    }

    private void OnDestroy()
    {
        resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);
        displayModeDropdown.onValueChanged.RemoveListener(ChangeDisplayMode);
    }
}
