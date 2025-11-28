using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    private Rigidbody rb;
    private float speed = 3f;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        // Read speed from config manager if available
        if (ConfigManager.Instance != null) {
            speed = ConfigManager.Instance.PlayerSpeed;
        } else {
            Debug.LogWarning("ConfigManager not found. Using default speed.");
        }
    }

    private void FixedUpdate() {
        // Get input (WASD or arrow keys)
        float h = Input.GetAxisRaw("Horizontal"); // A/D or left/right
        float v = Input.GetAxisRaw("Vertical");   // W/S or up/down

        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f) input = input.normalized;

        Vector3 move = input * speed * Time.fixedDeltaTime;
        Vector3 targetPos = rb.position + move;
        rb.MovePosition(targetPos);
    }
}
