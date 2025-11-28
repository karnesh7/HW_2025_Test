using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI scoreText; // assign the ScoreText in the Canvas

    private int score = 0;

    // Track the pulpit that was last stepped on (to avoid double counting)
    private Pulpit lastPulpit = null;

    // Publicly visible pulpit the player is currently standing on
    public Pulpit CurrentPulpit { get; private set; } = null;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // optional: DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        UpdateScoreUI();
    }

    /// <summary>
    /// Call this when the player steps on a pulpit.
    /// It will update CurrentPulpit and increment the score only if it's a new pulpit.
    /// </summary>
    public void RegisterStepOn(Pulpit pulpit) {
        if (pulpit == null) return;

        // Update current pulpit reference (player is now on this pulpit)
        CurrentPulpit = pulpit;

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

    public int GetScore() => score;

    // Optional: helper to reset score (useful for testing)
    public void ResetScore() {
        score = 0;
        lastPulpit = null;
        CurrentPulpit = null;
        UpdateScoreUI();
    }
}
