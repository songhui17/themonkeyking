using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    private Transform _target;
    private Vector3 _offset;

    [Header("Follow Impl2")]
    private Animator _targetAnimator;
    public float angularClampSpeed = 1.0f;
    public Vector3 rotationPivot = Vector3.zero;
    private float horizontalDistance = 0;
    private float verticalDistance = 0;

    void Start() {
        var player = GameObject.FindWithTag("Player");
        _target = player.transform;

        _offset = _target.position - transform.position;
        StartImpl2();
    }

    void KeepOffset() {
        transform.position = _target.position - _offset;
    }

    void StartImpl2() {
        _targetAnimator = _target.GetComponent<Animator>();
        verticalDistance = _offset.y;
        _offset.y = 0;
        horizontalDistance = _offset.magnitude;
        FollowImpl2();
    }

    void FollowImpl1() {
        KeepOffset();
    }

    bool CanRotate() {
        if (_targetAnimator == null || _targetAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk") ||
            (_targetAnimator.IsInTransition(0) && _targetAnimator.GetNextAnimatorStateInfo(0).IsName("Walk"))) {
            return true;
        }
        else {
            return false;
        }
    }
    void FollowImpl2() {
        var v2Target = _target.TransformPoint(rotationPivot) - transform.position;
        v2Target.Normalize();
        if (v2Target == Vector3.zero) {
            return;
        }
        var newOffset = new Vector3(v2Target.x, 0, v2Target.z);
        newOffset.Normalize();
        newOffset *= horizontalDistance;
        newOffset.y = verticalDistance;
        transform.position = _target.position - newOffset;

        if (CanRotate()) {
            // 朝向时目标居中
            var targetRotation = Quaternion.LookRotation(v2Target, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * angularClampSpeed);
        }
    }

    void LateUpdate() {
        if (_target == null) {
            return;
        }

        FollowImpl2();
    }
}
