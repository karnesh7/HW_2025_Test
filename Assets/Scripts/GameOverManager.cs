using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour {
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameOverPanel;      // assign GameOverPanel
    public TextMeshProUGUI finalScoreText; // optional final score display

    private bool isGameOver = false;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void TriggerGameOver() {
        if (isGameOver) return;
        isGameOver = true;

        // Show UI
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Show final score if assigned
        if (finalScoreText != null && ScoreManager.Instance != null) {
            finalScoreText.text = $"Score: {ScoreManager.Instance.GetScore()}";
        }

        // Freeze game (simple approach)
        Time.timeScale = 0f;

        // Optionally disable player controls: find Doofus and disable PlayerController
        var player = GameObject.FindWithTag("Player");
        if (player != null) {
            var ctrl = player.GetComponent<PlayerController>();
            if (ctrl != null) ctrl.enabled = false;
        }

        Debug.Log("GameOverManager: Game Over triggered.");
    }

    public void Restart() {
        // Unfreeze
        Time.timeScale = 1f;
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
