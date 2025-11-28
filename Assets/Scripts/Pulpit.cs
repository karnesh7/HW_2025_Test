using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class Pulpit : MonoBehaviour {
    public float destroyTime { get; private set; } = -1f;
    public float spawnTrigger { get; private set; } = -1f;

    private bool spawnRequested = false;
    private float elapsed = 0f;

    [Header("UI")]
    public TextMeshPro countdownLabel;

    private bool initialized = false;

    // public flag so manager can know where the player is
    public bool PlayerOn { get; private set; } = false;

    private void Start() {
        if (!initialized) {
            // fallback safe defaults (shouldn't be used if manager always Initialize)
            float min = ConfigManager.Instance?.MinPulpitDestroy ?? 4f;
            float max = ConfigManager.Instance?.MaxPulpitDestroy ?? 5f;
            float desiredOverlap = ConfigManager.Instance?.PulpitSpawnTime ?? 2.5f;
            destroyTime = Random.Range(min, max);
            spawnTrigger = Mathf.Max(min, destroyTime - desiredOverlap);
            initialized = true;
            Debug.Log($"Pulpit fallback init @ {transform.position} destroy={destroyTime:F2} spawnAt={spawnTrigger:F2}");
        }

        if (countdownLabel == null) countdownLabel = GetComponentInChildren<TextMeshPro>();

        StartCoroutine(LifecycleCoroutine());
    }

    // Called by manager to provide deterministic timings
    public void Initialize(float destroyDuration, float spawnAtSeconds) {
        destroyTime = Mathf.Max(0.05f, destroyDuration);
        spawnTrigger = Mathf.Clamp(spawnAtSeconds, 0f, destroyTime - 0.01f);
        initialized = true;
        Debug.Log($"Pulpit.Initialize @ {transform.position} destroy={destroyTime:F2} spawnAt={spawnTrigger:F2}");
    }

    private IEnumerator LifecycleCoroutine() {
        elapsed = 0f;
        spawnRequested = false;

        while (elapsed < destroyTime) {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0f, destroyTime - elapsed);

            if (countdownLabel != null) {
                countdownLabel.text = remaining.ToString("F1");
            }

            if (!spawnRequested && elapsed >= spawnTrigger) {
                spawnRequested = true;
                Debug.Log($"Pulpit @ {transform.position} reached spawnTrigger (elapsed={elapsed:F2} spawnAt={spawnTrigger:F2}) -> requesting spawn");
                PulpitManager.Instance?.RequestSpawnAdjacent(this);
            }

            yield return null;
        }

        Debug.Log($"Pulpit @ {transform.position} dying now (destroyTime={destroyTime:F2})");
        PulpitManager.Instance?.UnregisterPulpit(this);
        Destroy(gameObject);
    }

    // Single OnTriggerEnter + OnTriggerExit pair (no duplicates)
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerOn = true;
            ScoreManager.Instance?.RegisterStepOn(this); // register once
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerOn = false;
        }
    }

    // Removed OnDestroy Unregister to avoid double-unregisters (coroutine already does that)
    // If you need a safety unregister, add a guard to avoid double-calls.

    public Vector3 CenterPosition() => transform.position;
}
