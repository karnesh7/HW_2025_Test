using UnityEngine;

[RequireComponent(typeof(Transform))]
public class PlayerFallDetector : MonoBehaviour {
    [Tooltip("Y threshold below which player is considered fallen and game over triggers.")]
    public float fallYThreshold = -5f;

    private bool triggered = false;

    private void Update() {
        if (triggered) return;

        if (transform.position.y <= fallYThreshold) {
            triggered = true;
            Debug.Log($"PlayerFallDetector: player fell below {fallYThreshold}. Triggering Game Over.");
            GameOverManager.Instance?.TriggerGameOver();
        }
    }
}
