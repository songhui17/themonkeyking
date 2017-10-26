
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ICancelHandler, IMoveHandler {
    public enum JoyStickState {
        Idle,
        Moving,
    }
    public JoyStickState joystickState = JoyStickState.Idle;
    public Transform pivot;
    public Transform aimingCircle;

    // 直接引用玩家
    private PlayerInput playerInput;

    void Start() {
        var player = GameObject.FindWithTag("Player");
        playerInput = player.GetComponent<PlayerInput>();
    }

    void UpdateJoystick(Vector3 localVector) {
        var localRotation = Quaternion.identity;
        if (localVector.magnitude > 0.001) {
            localRotation = Quaternion.LookRotation(Vector3.forward, localVector);
        }

        aimingCircle.localRotation = localRotation;
        // 待办：无需每一帧都更新
        if (playerInput != null) {
            playerInput.HandleJoyStick(localVector.x, localVector.y);
        }
    }

    void UpdateIdle() {
        var horizontalAxis = Input.GetAxis("Horizontal");
        var verticalAxis = Input.GetAxis("Vertical");
        var localVector = new Vector3(horizontalAxis, verticalAxis, 0);

        UpdateJoystick(localVector);
    }

    void UpdateMoving() {
        if (pivot == null || aimingCircle == null) {
            return;
        }

        var localVector = Vector3.zero;

        var mousePosition = Input.mousePosition;
        var localPosition = pivot.InverseTransformPoint(mousePosition);
        if (localPosition.magnitude > 0.001) {
            // Debug.Log(localPosition);
            localVector = localPosition.normalized;
        }

        UpdateJoystick(localVector);
    }

    void Update() {
        if (joystickState != JoyStickState.Moving) {
            UpdateIdle();
        }
        else {
            UpdateMoving();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        // Debug.Log("PointerDown");
        joystickState = JoyStickState.Moving;
    }

    public void OnPointerUp(PointerEventData eventData) {
        // Debug.Log("PointerUp");
        joystickState = JoyStickState.Idle;

        if (aimingCircle != null) {
            aimingCircle.localRotation = Quaternion.identity;
        }

        if (playerInput != null) {
            playerInput.HandleJoyStick(0.0f, 0.0f);  // Update不会在Idle状态HandleJoyStick
        }
    }

    public void OnMove(AxisEventData eventData) {
        if (joystickState != JoyStickState.Moving) {
            return;
        }
    }

    public void OnCancel(BaseEventData eventData) {
        // Debug.Log("Cancel");
    }
}
