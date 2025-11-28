using System.Collections.Generic;
using UnityEngine;

public class PulpitManager : MonoBehaviour {
    public static PulpitManager Instance { get; private set; }
    public static bool IsInitialized => Instance != null;

    [Header("References")]
    public GameObject pulpitPrefab;
    public Transform pulpitParent;

    [Header("Tuning")]
    public bool enforceMinSafeLifetime = true;
    public float minSafeLifetime = 1.2f;    // extra travel time after overlap
    public float debugLogSeconds = 0f;      // set >0 to throttle debug spam

    [Header("Spawn behavior")]
    [Tooltip("When a pulpit is removed, block spawning at that exact position for this many seconds")]
    public float recentlyRemovedBlockDuration = 1.5f;

    // internal tracking of the most-recently removed pulpit position
    private Vector2 lastRemovedPosXZ = new Vector2(float.NaN, float.NaN);
    private float lastRemovedTime = -999f;

    private readonly List<Pulpit> activePulpits = new List<Pulpit>();
    private readonly HashSet<Pulpit> spawnRequestedFrom = new HashSet<Pulpit>();

    private float step = 9f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        if (pulpitParent == null) {
            var go = new GameObject("Pulpits");
            pulpitParent = go.transform;
        }

        // sanity: print config values
        Debug.Log($"PULPIT MANAGER START - Min:{ConfigManager.Instance?.MinPulpitDestroy:F2} Max:{ConfigManager.Instance?.MaxPulpitDestroy:F2} OverlapLead:{ConfigManager.Instance?.PulpitSpawnTime:F2} minSafeLifetime:{minSafeLifetime:F2}");

        SpawnPulpitAt(new Vector3(0f, 0.1f, 0f));
    }

    public void RequestSpawnAdjacent(Pulpit source) {
        if (source == null) return;

        // Debounce duplicate requests from same source
        if (spawnRequestedFrom.Contains(source)) {
            Debug.Log($"PulpitManager: Ignored duplicate spawn request from {source.transform.position}");
            return;
        }

        spawnRequestedFrom.Add(source);
        Debug.Log($"PulpitManager: Spawn requested by pulpit at {source.transform.position} (activeCount={activePulpits.Count})");

        // If we already have <2 pulpits spawn immediately, otherwise leave Unregister to fill.
        if (activePulpits.Count < 2) {
            SpawnAdjacentTo(source);
        } else {
            Debug.Log("PulpitManager: Already at max pulpits; spawn will occur when one dies.");
        }
    }

    public void UnregisterPulpit(Pulpit p) {
        if (p == null) return;
        if (activePulpits.Contains(p)) activePulpits.Remove(p);
        spawnRequestedFrom.Remove(p);

        Debug.Log($"PulpitManager: Unregistered pulpit at {p.transform.position}. Active now: {activePulpits.Count}");

        // record the removed position (XZ) and timestamp so we can avoid spawning there immediately
        lastRemovedPosXZ = new Vector2(p.transform.position.x, p.transform.position.z);
        lastRemovedTime = Time.time;

        // Determine preferred anchor pulpit to spawn adjacent to:
        // 1) If the player is on a pulpit, prefer that one (ScoreManager owns CurrentPulpit).
        // 2) Else prefer the pulpit that requested spawn (if any and still active).
        // 3) Else use first active pulpit.
        Pulpit preferred = null;

        // 1) player pulpit (from ScoreManager)
        if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentPulpit != null && activePulpits.Contains(ScoreManager.Instance.CurrentPulpit)) {
            preferred = ScoreManager.Instance.CurrentPulpit;
        }

        // 2) any pulpit in spawnRequestedFrom set (choose arbitrary active one)
        if (preferred == null) {
            foreach (var req in spawnRequestedFrom) {
                if (req != null && activePulpits.Contains(req)) {
                    preferred = req;
                    break;
                }
            }
        }

        // 3) fallback to first active pulpit
        if (preferred == null && activePulpits.Count > 0) {
            preferred = activePulpits[0];
        }

        // Now restore up to 2 pulpits but spawn adjacent to the 'preferred' reference
        int needed = 2 - activePulpits.Count;
        for (int i = 0; i < needed; i++) {
            if (preferred == null) {
                // no active pulpits — spawn at origin
                SpawnPulpitAt(new Vector3(0f, 0.1f, 0f));
            } else {
                SpawnAdjacentTo(preferred);
            }
        }
    }

    private bool IsPositionOccupied(Vector3 pos) {
        foreach (var p in activePulpits) {
            if (p == null) continue;
            if (Mathf.Approximately(p.transform.position.x, pos.x) &&
                Mathf.Approximately(p.transform.position.z, pos.z)) {
                return true;
            }
        }
        return false;
    }

    private Pulpit SpawnPulpitAt(Vector3 position) {
        if (pulpitPrefab == null) {
            Debug.LogError("PulpitManager: pulpitPrefab not assigned.");
            return null;
        }

        GameObject go = Instantiate(pulpitPrefab, position, Quaternion.identity, pulpitParent);
        Pulpit pulpit = go.GetComponent<Pulpit>() ?? go.AddComponent<Pulpit>();

        // manager decides exact timing deterministically
        float min = ConfigManager.Instance?.MinPulpitDestroy ?? 4f;
        float max = ConfigManager.Instance?.MaxPulpitDestroy ?? 5f;
        float desiredOverlap = ConfigManager.Instance?.PulpitSpawnTime ?? 2.5f;

        // pick destroy time randomly between min and max, then enforce safe minimum
        float destroyT = Random.Range(min, max);

        // Ensure destroy time gives at least desiredOverlap + minSafeLifetime
        float minAllowedDestroy = desiredOverlap + minSafeLifetime;
        if (enforceMinSafeLifetime && destroyT < minAllowedDestroy) {
            destroyT = minAllowedDestroy;
        }

        // Now set spawnAt so it occurs exactly 'desiredOverlap' seconds before destroy
        float spawnAt = destroyT - desiredOverlap;
        spawnAt = Mathf.Clamp(spawnAt, 0.05f, Mathf.Max(0.05f, destroyT - 0.05f));

        pulpit.Initialize(destroyT, spawnAt);

        activePulpits.Add(pulpit);

        Debug.Log($"Spawned pulpit at {position}  -> destroy={destroyT:F2}s spawnAt={spawnAt:F2}s  (active={activePulpits.Count})");
        return pulpit;
    }

    private void SpawnAdjacentTo(Pulpit source) {
        if (pulpitPrefab == null) {
            Debug.LogError("PulpitManager: pulpitPrefab not assigned.");
            return;
        }

        Vector3 srcPos = source.CenterPosition();

        Vector3[] candidates = new Vector3[] {
            srcPos + new Vector3(step, 0f, 0f),
            srcPos + new Vector3(-step, 0f, 0f),
            srcPos + new Vector3(0f, 0f, step),
            srcPos + new Vector3(0f, 0f, -step),
        };

        Shuffle(candidates);

        bool blockRecent = !float.IsNaN(lastRemovedPosXZ.x) && (Time.time - lastRemovedTime) < recentlyRemovedBlockDuration;

        foreach (var pos in candidates) {
            // if this candidate is exactly the recently removed spot and blocking is active, skip it
            if (blockRecent) {
                if (Mathf.Approximately(pos.x, lastRemovedPosXZ.x) && Mathf.Approximately(pos.z, lastRemovedPosXZ.y)) {
                    // prefer other sides instead of reinstantiating at the removed spot
                    Debug.Log($"PulpitManager: Skipping candidate at recently removed spot {pos}");
                    continue;
                }
            }

            if (!IsPositionOccupied(pos)) {
                SpawnPulpitAt(pos);
                return;
            }
        }    

        // build list of manhattan-distance-2 positions (excluding immediate neighbors and center)
        var radius2 = new List<Vector3>();
        for (int dx = -2; dx <= 2; dx++) {
            for (int dz = -2; dz <= 2; dz++) {
                int manhattan = Mathf.Abs(dx) + Mathf.Abs(dz);
                if (manhattan == 0 || manhattan > 2) continue;
                // skip immediate neighbors (we already checked them)
                if (manhattan == 1) continue;
                Vector3 pos = srcPos + new Vector3(dx * step, 0f, dz * step);
                radius2.Add(pos);
            }
        }

        // shuffle radius2 properly by converting to array, shuffling array, and using that
        Vector3[] radius2Arr = radius2.ToArray();
        Shuffle(radius2Arr);
        foreach (var pos in radius2Arr) {
            if (!IsPositionOccupied(pos)) {
                SpawnPulpitAt(pos);
                return;
            }
        }

        Debug.LogWarning("PulpitManager: Could not find free adjacent or near-adjacent position to spawn pulpit.");
    }

    private void Shuffle<T>(T[] array) {
        for (int i = array.Length - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            T tmp = array[i];
            array[i] = array[j];
            array[j] = tmp;
        }
    }

    public int ActivePulpitCount() => activePulpits.Count;
}