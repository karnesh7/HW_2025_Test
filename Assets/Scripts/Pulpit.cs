using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class Pulpit : MonoBehaviour {
    public float destroyTime { get; private set; }
    public float spawnTrigger { get; private set; }

    private bool spawnRequested = false;
    private float elapsed = 0f;

    // Countdown UI (TextMeshPro - 3D)
    [Header("UI")]
    public TextMeshPro countdownLabel; // assign in prefab (child TextMeshPro)

    // Keep track if player is currently on this pulpit
    private bool playerOn = false;

    private void Start() {
        float min = ConfigManager.Instance?.MinPulpitDestroy ?? 4f;
        float max = ConfigManager.Instance?.MaxPulpitDestroy ?? 5f;

        destroyTime = Random.Range(min, max);
        spawnTrigger = Random.Range(min, max);

        if (spawnTrigger > destroyTime) spawnTrigger = Random.Range(min, destroyTime);

        // If countdownLabel not set in prefab, try to find one
        if (countdownLabel == null) {
            countdownLabel = GetComponentInChildren<TextMeshPro>();
            if (countdownLabel == null) {
                Debug.LogWarning("Pulpit: No TextMeshPro countdown label found in children.");
            }
        }

        StartCoroutine(LifecycleCoroutine());
    }

    private IEnumerator LifecycleCoroutine() {
        while (elapsed < destroyTime) {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0f, destroyTime - elapsed);

            // Update countdown UI (display to 1 decimal)
            if (countdownLabel != null) {
                countdownLabel.text = remaining.ToString("F1");
                // optional: change color when low
                if (remaining <= 1.5f) {
                    countdownLabel.color = Color.red;
                } else if (remaining <= 3f) {
                    countdownLabel.color = Color.yellow;
                } else {
                    countdownLabel.color = Color.white;
                }
                // Make label face camera (billboard)
                if (Camera.main != null) {
                    countdownLabel.transform.rotation = Quaternion.LookRotation(countdownLabel.transform.position - Camera.main.transform.position);
                }
            }

            if (!spawnRequested && elapsed >= spawnTrigger) {
                spawnRequested = true;
                PulpitManager.Instance?.RequestSpawnAdjacent(this);
            }

            yield return null;
        }

        // Lifetime finished — unregister and destroy
        PulpitManager.Instance?.UnregisterPulpit(this);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            // Mark player on this pulpit
            playerOn = true;
            // Inform ScoreManager that player stepped on this pulpit
            ScoreManager.Instance?.RegisterStepOn(this);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerOn = false;
        }
    }

    private void OnDestroy() {
        if (PulpitManager.IsInitialized) PulpitManager.Instance?.UnregisterPulpit(this);
    }

    public Vector3 CenterPosition() {
        return transform.position;
    }
}
