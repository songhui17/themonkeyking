using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform Target;
    private Vector3 _offset;

    void Start() {
        _offset = Target.position - transform.position;
    }

    void KeepOffset() {
        transform.position = Target.position - _offset;
    }

    void LateUpdate() {
        if (Target == null) {
            return;
        }

        KeepOffset();
    }
}
