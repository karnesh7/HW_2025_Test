using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerData {
    public float speed = 3f;
}

[Serializable]
public class PulpitData {
    public float min_pulpit_destroy_time = 4f;
    public float max_pulpit_destroy_time = 5f;
    public float pulpit_spawn_time = 2.5f;
}

[Serializable]
public class DoofusDiary {
    public PlayerData player_data = new PlayerData();
    public PulpitData pulpit_data = new PulpitData();
}

public class ConfigManager : MonoBehaviour {
    public static ConfigManager Instance { get; private set; }

    public DoofusDiary Diary { get; private set; }

    // Exposed convenience properties
    public float PlayerSpeed => Diary?.player_data?.speed ?? 3f;
    public float MinPulpitDestroy => Diary?.pulpit_data?.min_pulpit_destroy_time ?? 4f;
    public float MaxPulpitDestroy => Diary?.pulpit_data?.max_pulpit_destroy_time ?? 5f;
    public float PulpitSpawnTime => Diary?.pulpit_data?.pulpit_spawn_time ?? 2.5f;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDiary();
    }

    private void LoadDiary() {
        string fileName = "DoofusDiary.json";
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        try {
            string json;
#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android streamingAssetsPath is compressed; you'd need WWW/UnityWebRequest, but editor/testing won't use android for this assignment
            Debug.LogWarning("Android platform: StreamingAssets read may need UnityWebRequest. Using fallback defaults.");
            json = "";
#else
            if (!File.Exists(path)) {
                Debug.LogError($"Config file not found at: {path}. Using default values.");
                Diary = new DoofusDiary();
                return;
            }
            json = File.ReadAllText(path);
#endif
            Diary = JsonUtility.FromJson<DoofusDiary>(json);
            if (Diary == null) {
                Debug.LogError("Failed to parse DoofusDiary.json — using defaults.");
                Diary = new DoofusDiary();
            } else {
                Debug.Log($"DoofusDiary loaded. Player speed: {PlayerSpeed}, Pulpit spawn time: {PulpitSpawnTime}");
            }
        } catch (Exception ex) {
            Debug.LogError($"Exception reading DoofusDiary.json: {ex.Message}. Using defaults.");
            Diary = new DoofusDiary();
        }
    }
}
