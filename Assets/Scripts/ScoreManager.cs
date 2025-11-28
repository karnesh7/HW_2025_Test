using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI scoreText; // assign ScoreText (UI canvas)

    private int score = 0;

    // Track the pulpit that was last stepped on (to avoid double counting)
    private Pulpit lastPulpit = null;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // optionally DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        UpdateScoreUI();
    }

    public void RegisterStepOn(Pulpit pulpit) {
        if (pulpit == null) return;

        // If stepping onto same pulpit again, do not increment
        if (lastPulpit == pulpit) return;

        // New pulpit — increment score
        score += 1;
        lastPulpit = pulpit;
        UpdateScoreUI();

        Debug.Log($"ScoreManager: stepped on new pulpit. Score = {score}");
    }

    private void UpdateScoreUI() {
        if (scoreText != null) {
            scoreText.text = $"Score: {score}";
        }
    }

    // Optional accessor
    public int GetScore() => score;
}
