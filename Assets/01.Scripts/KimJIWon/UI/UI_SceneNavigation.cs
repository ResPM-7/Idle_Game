using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_SceneNavigation : MonoBehaviour
{
    public const string MainScenePath = "Assets/00.Scenes/MainScene.unity";
    public const string TitleScenePath = "Assets/00.Scenes/KimJiWon/TitleScene.unity";
    private static bool loading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState() => loading = false;

    public void GoToMainScene() => Load(MainScenePath);
    public void GoToTitleScene() => Load(TitleScenePath);

    private static void Load(string path)
    {
        if (loading) return;
        if (!Application.CanStreamedLevelBeLoaded(path))
        {
            Debug.LogError($"Scene is not enabled in Build Settings: {path}");
            return;
        }
        loading = true;
        // 씬 오브젝트가 파괴되기 전에 마지막 편성/재화를 저장합니다.
        if (GameSaveService.instance != null) GameSaveService.instance.SaveNow();
        float previousScale = Time.timeScale;
        Time.timeScale = 1f;
        try
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            operation.completed += _ => 
            {
                Time.timeScale = 1f;
                loading = false;

                
            };
        }
        catch
        {
            loading = false;
            Time.timeScale = previousScale;
            throw;
        }
    }

    public void QuitGame()
    {
        if (GameSaveService.instance != null) GameSaveService.instance.SaveNow();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
