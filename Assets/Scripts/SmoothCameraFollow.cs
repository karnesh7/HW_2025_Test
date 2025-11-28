using UnityEngine;

[ExecuteAlways]
public class SmoothCameraFollow : MonoBehaviour {
    [Tooltip("Target to follow (Doofus)")]
    public Transform target;

    [Tooltip("Offset from target in local space (useful to position camera above/back)")]
    public Vector3 offset = new Vector3(0f, 6f, -10f);

    [Tooltip("How quickly the camera follows the target. Higher = snappier")]
    [Range(0.01f, 10f)]
    public float followSpeed = 5f;

    [Tooltip("Whether the camera should look at the target")]
    public bool lookAtTarget = true;

    [Tooltip("Minimum Y height to avoid camera going below ground")]
    public float minY = 1f;

    private void Reset() {
        // sensible defaults
        offset = new Vector3(0f, 6f, -10f);
        followSpeed = 5f;
        lookAtTarget = true;
        minY = 1f;
    }

    private void LateUpdate() {
        if (target == null) return;

        // Desired world position based on target and offset
        Vector3 desiredPos = target.position + offset;

        // clamp Y so camera doesn't go underground
        if (desiredPos.y < minY) desiredPos.y = minY;

        // Smooth interpolate current position to desired
        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));

        if (lookAtTarget) {
            transform.LookAt(target.position + Vector3.up * 0.5f); // slight look-up
        }
    }
}
