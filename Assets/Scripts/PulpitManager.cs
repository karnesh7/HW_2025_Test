using System.Collections.Generic;
using UnityEngine;

public class PulpitManager : MonoBehaviour {
    public static PulpitManager Instance { get; private set; }
    public static bool IsInitialized => Instance != null;

    [Header("References")]
    public GameObject pulpitPrefab; // assign Pulpit.prefab here
    public Transform pulpitParent;  // optional parent for spawned pulpits

    // track active pulpits
    private readonly List<Pulpit> activePulpits = new List<Pulpit>();

    // grid step = pulpit size (we used 9 units)
    private float step = 9f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        // If pulpitParent not set, create a parent container
        if (pulpitParent == null) {
            var go = new GameObject("Pulpits");
            pulpitParent = go.transform;
        }

        // Spawn initial pulpit at origin
        SpawnPulpitAt(new Vector3(0f, 0.1f, 0f));
    }

    public void RequestSpawnAdjacent(Pulpit source) {
        // If already 2 pulpits, ignore request (manager enforces max 2)
        if (activePulpits.Count >= 2) return;

        SpawnAdjacentTo(source);
    }

    public void UnregisterPulpit(Pulpit p) {
        if (p == null) return;
        if (activePulpits.Contains(p)) {
            activePulpits.Remove(p);
        }

        // After removal, if we now have fewer than 2 pulpits, spawn replacement(s)
        // Aim to restore count to 2 immediately (or as many as practical).
        // If there is one remaining pulpit, spawn adjacent to it;
        // if zero remain (edge case), spawn at origin.
        int needed = 2 - activePulpits.Count;
        for (int i = 0; i < needed; i++) {
            if (activePulpits.Count == 0) {
                // spawn a pulpit at origin
                SpawnPulpitAt(new Vector3(0f, 0.1f, 0f));
            } else if (activePulpits.Count == 1) {
                // spawn adjacent to the single remaining pulpit
                SpawnAdjacentTo(activePulpits[0]);
            } else {
                // already at or above desired count
                break;
            }
        }
    }

    private void SpawnAdjacentTo(Pulpit source) {
        if (pulpitPrefab == null) {
            Debug.LogError("PulpitManager: pulpitPrefab not assigned.");
            return;
        }

        Vector3 srcPos = source.CenterPosition();

        // Candidate positions (N, S, E, W)
        Vector3[] candidates = new Vector3[] {
            srcPos + new Vector3(step, 0f, 0f),
            srcPos + new Vector3(-step, 0f, 0f),
            srcPos + new Vector3(0f, 0f, step),
            srcPos + new Vector3(0f, 0f, -step),
        };

        // Shuffle candidates to randomize placement
        Shuffle(candidates);

        foreach (var pos in candidates) {
            if (!IsPositionOccupied(pos)) {
                SpawnPulpitAt(pos);
                return;
            }
        }

        // If all adjacent positions occupied (unlikely with max 2), try other nearby positions by expanding radius
        // Try two-step positions
        foreach (var pos in candidates) {
            var fallback = pos + new Vector3(step, 0f, 0f); // arbitrary extra offset
            if (!IsPositionOccupied(fallback)) {
                SpawnPulpitAt(fallback);
                return;
            }
        }

        Debug.LogWarning("PulpitManager: Could not find free adjacent position to spawn pulpit.");
    }

    private bool IsPositionOccupied(Vector3 pos) {
        foreach (var p in activePulpits) {
            if (p == null) continue;
            // compare XZ positions (ignore Y)
            if (Mathf.Approximately(p.transform.position.x, pos.x) &&
                Mathf.Approximately(p.transform.position.z, pos.z)) {
                return true;
            }
        }
        return false;
    }

    private Pulpit SpawnPulpitAt(Vector3 position) {
        if (pulpitPrefab == null) {
            Debug.LogError("PulpitManager: pulpitPrefab is null.");
            return null;
        }

        GameObject go = Instantiate(pulpitPrefab, position, Quaternion.identity, pulpitParent);
        Pulpit pulpit = go.GetComponent<Pulpit>();
        if (pulpit == null) pulpit = go.AddComponent<Pulpit>();

        activePulpits.Add(pulpit);
        return pulpit;
    }

    // Utility function to shuffle an array (Fisher-Yates)
    private void Shuffle<T>(T[] array) {
        for (int i = array.Length - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            T tmp = array[i];
            array[i] = array[j];
            array[j] = tmp;
        }
    }

    // Expose active count (useful later)
    public int ActivePulpitCount() => activePulpits.Count;
}
