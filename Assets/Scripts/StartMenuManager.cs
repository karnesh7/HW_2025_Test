using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Small manager for the Start screen. Attach to an empty GameObject (StartMenuManager).
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    [Tooltip("Name of the main gameplay scene to load")]
    public string mainSceneName = "Main";

    private void Awake() {
        // Make sure game isn't paused if we return to menu (safety)
        Time.timeScale = 1f;
    }

    // Called from Start button
    public void StartGame() {
        // Reset score (if ScoreManager exists in this active scene or stays between scenes)
        if (ScoreManager.Instance != null) {
            ScoreManager.Instance.ResetScore();
        }

        // Load main scene
        SceneManager.LoadScene(mainSceneName);
    }

    // Called from Quit button
    public void QuitGame() {
#if UNITY_EDITOR
        // Stop Play mode in Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
