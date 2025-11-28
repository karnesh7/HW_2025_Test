using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pulpit : MonoBehaviour {
    public float destroyTime { get; private set; }
    public float spawnTrigger { get; private set; }

    private bool spawnRequested = false;
    private float elapsed = 0f;

    private void Start() {
        // Get min/max from config (ConfigManager provides defaults)
        float min = ConfigManager.Instance?.MinPulpitDestroy ?? 4f;
        float max = ConfigManager.Instance?.MaxPulpitDestroy ?? 5f;

        destroyTime = Random.Range(min, max);
        spawnTrigger = Random.Range(min, max);

        // Ensure spawnTrigger is not > destroyTime (optional). The assignment wording says spawnTrigger is random between y and z; it can be > destroy time — but that would never cause spawn.
        // To ensure spawn happens before destroy in most cases, clamp spawnTrigger to be <= destroyTime:
        if (spawnTrigger > destroyTime) spawnTrigger = Random.Range(min, destroyTime);

        // Start the self-destruction coroutine
        StartCoroutine(LifecycleCoroutine());
    }

    private IEnumerator LifecycleCoroutine() {
        while (elapsed < destroyTime) {
            elapsed += Time.deltaTime;

            // Request spawn only once when we reach spawnTrigger
            if (!spawnRequested && elapsed >= spawnTrigger) {
                spawnRequested = true;
                // Ask the manager to spawn adjacent to this pulpit (manager will check active count)
                PulpitManager.Instance?.RequestSpawnAdjacent(this);
            }

            yield return null;
        }

        // Lifetime finished — tell manager and destroy
        PulpitManager.Instance?.UnregisterPulpit(this);
        Destroy(gameObject);
    }

    // For safety, if pulpit is destroyed externally, ensure manager is updated
    private void OnDestroy() {
        // We already call Unregister before Destroy in coroutine; but in case Destroy is called elsewhere:
        if (PulpitManager.IsInitialized) PulpitManager.Instance?.UnregisterPulpit(this);
    }

    // Helper: get pulpit center position
    public Vector3 CenterPosition() {
        return transform.position;
    }
}
