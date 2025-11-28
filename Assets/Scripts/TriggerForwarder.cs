using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerForwarder : MonoBehaviour {
    public Pulpit pulpitParent;

    private void Awake() {
        if (pulpitParent == null) pulpitParent = GetComponentInParent<Pulpit>();
    }

    private void OnTriggerEnter(Collider other) {
        if (pulpitParent != null) pulpitParent.HandleTriggerEnterFromChild(other);
    }

    private void OnTriggerExit(Collider other) {
        if (pulpitParent != null) pulpitParent.HandleTriggerExitFromChild(other);
    }
}
