using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class MovementStrategy {
// 
// }
// 
// public class MovementImpl : MovementStrategy {
//     public Animator animator;
//     public CharacterController controller;
//     public Transform transform;
// 
//     public MovementImpl(PlayerInput playerInput) {
//         transform = playerInput.transform;
//         animator = playerInput.animator;
//         controller = playerInput.controller;
//     }
// 
//     public void UpdateRun(float horizontalAxis, float verticalAxis) {
//         // var horizontalAxis = Input.GetAxis("Horizontal");
//         // var verticalAxis = Input.GetAxis("Vertical");
//         var v = transform.forward * verticalAxis * verticalSpeed + transform.right * horizontalAxis * horizontalSpeed;
//         velocity = v;
//         controller.SimpleMove(v);
// 
//         animator.SetFloat("speed", v.magnitude);
//     }
// }


public class PlayerInput : MonoBehaviour {
    public Animator animator;
    public CharacterController controller;

    public const string IDLE_NAME = "Idle";
    public const string WALK_NAME = "Walk";

    // 调整移动速度
    [Header("Movement Impl1")]
    public float horizontalSpeed = 1.0f;
    public float verticalSpeed = 1.0f;

    [Header("Movement Impl2")]
    public float speed = 1.0f;
    public float angularClampSpeed = 1.0f;
    public float walkAnimationSpeedMultiplier = 1.0f;
    public float angularClampSpeedOnAttack = 0.5f;

    [Header("Movement Impl3")]
    public Transform cameraTransform;

    // 角色属性
    public float attackSpeed = 1.0f;

    // 角色移动
    public enum MovementState {
        // Idle,
        Run,
        Jump
    }
    public MovementState movementState = MovementState.Run;
    public Vector3 velocity;
    public Vector3 localVelocity;  // 上一次摇杆输入

    public float maxJumpingSpeed = 1f;
    public float jumpAcc = -0.1f;

    public float jumpingSpeed;

    // 状态机状态
    public enum AnimatorState {
        Idle,
        Attack,
        Other,
    }
    public AnimatorState animatorState;

    // 角色战斗
    public MKBCollider mkb;  // 待办：分离武器和碰撞器

    public enum AttackSeqState {
        Idle,
        Attack,  // 处于攻击状态
    }
    public AttackSeqState attackSeqState = AttackSeqState.Idle;
    public int attackSeqCount = 0;

    // Use this for initialization
    void Start () {
        InitAnimatorState();
        InitMovementState();
    }

    void InitAnimatorState() {
        animatorState = AnimatorState.Idle;
    }

#region MovementState

    void InitMovementState() {
        if (controller.isGrounded) {
            EnterJump();
        }
        else {
            movementState = MovementState.Run;
        }
    }

    bool GetJumpButton() {
        return Input.GetButton("Jump");
    }
    
    void EnterJump() {
        jumpingSpeed = maxJumpingSpeed;
        movementState = MovementState.Jump;
    }

    void UpdateJumpIml1() {
        var v = jumpingSpeed + Time.deltaTime * jumpAcc;
        var upDelta = Vector3.up * (v + jumpingSpeed) / 2 * Time.deltaTime;
        var horizontalDelta = velocity * Time.deltaTime;
        controller.Move(upDelta + horizontalDelta);

        animator.SetFloat("speed", 0.2f);  // 待办：跳跃和飞行动作
        jumpingSpeed = v;
    }

    Vector3 GetHorizontalJumpVelocity() {
        var v = localVelocity;
        v = GetCameraMatrix(cameraTransform).MultiplyVector(v);
        v.y = 0;
        v.Normalize();
        v *= speed;
        return v;
    }

    void UpdateJumpImpl3() {
        var v = jumpingSpeed + Time.deltaTime * jumpAcc;
        var upDelta = Vector3.up * (v + jumpingSpeed) / 2 * Time.deltaTime;
        var velocity = GetHorizontalJumpVelocity();
        var horizontalDelta = velocity * Time.deltaTime;
        controller.Move(upDelta + horizontalDelta);

        animator.SetFloat("speed", 0.2f);  // 待办：跳跃和飞行动作
        jumpingSpeed = v;

        // 跳跃是同样转向
        var targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * angularClampSpeed);
    }

    void UpdateJump() {
        UpdateJumpImpl3();
    }

    void UpdateRunImpl(float horizontalAxis, float verticalAxis) {
        // var horizontalAxis = Input.GetAxis("Horizontal");
        // var verticalAxis = Input.GetAxis("Vertical");
        var v = transform.forward * verticalAxis * verticalSpeed + transform.right * horizontalAxis * horizontalSpeed;
        velocity = v;
        controller.SimpleMove(v);

        animator.SetFloat("speed", v.magnitude);
    }

    void DoUpdateRunImpl23(float horizontalAxis, float verticalAxis, Matrix4x4 matrix, float angularClampSpeed, bool updatePosition=true) {
        var v = new Vector3(horizontalAxis, 0, verticalAxis);
        if (v.sqrMagnitude * speed * speed < 0.001f) { // speed < 0.1f
            animator.SetFloat("speed", 0.0f);
            localVelocity = Vector3.zero;
        }
        else {
            localVelocity = v;

            v = matrix.MultiplyVector(v);
            v.Normalize();

            v *= speed;
            velocity = v;

            var targetRotation = Quaternion.LookRotation(v, Vector3.up);
            // transform.rotation = targetRotation
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * angularClampSpeed);

            if (updatePosition) {
                controller.SimpleMove(v);
            }

            animator.SetFloat("speed", speed * walkAnimationSpeedMultiplier);
        }
    }

    // 固定速度向摇杆方向移动
    void UpdateRunImpl2(float horizontalAxis, float verticalAxis, bool updatePosition=true) {
        DoUpdateRunImpl23(horizontalAxis, verticalAxis, Matrix4x4.identity, angularClampSpeed, updatePosition:updatePosition);
    }

    Matrix4x4 GetCameraMatrix(Transform camera) {
        var forward = camera.forward;
        var up = Vector3.up;
        var right = Vector3.Cross(up, forward);
        right.Normalize();
        if (right == Vector3.zero) {
            // 垂直朝向，站在原地吧
            return Matrix4x4.zero;
        }
        else {
            forward = Vector3.Cross(right, up);
            forward.Normalize();
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetColumn(0, right);
            matrix.SetColumn(1, up);
            matrix.SetColumn(2, forward);
            // Debug.LogFormat("forward:{0}, right:{1}, up:{2}", forward, right, up);
            // Debug.Log(matrix);
            return matrix;
        }
    }

    void UpdateRunImpl3(float horizontalAxis, float verticalAxis, float angularClampSpeed, bool updatePosition=true) {
        DoUpdateRunImpl23(horizontalAxis, verticalAxis, GetCameraMatrix(cameraTransform), angularClampSpeed, updatePosition:updatePosition);
    }

    void UpdateRun(float horizontalAxis, float verticalAxis, float angularClampSpeed, bool updatePosition=true) {
        // UpdateRunImpl2(horizontalAxis, verticalAxis, updatePosition);
        UpdateRunImpl3(horizontalAxis, verticalAxis, angularClampSpeed, updatePosition);
    }

    void UpdateMovement(float horizontalAxis, float verticalAxis) {
        if (movementState == MovementState.Run) {
            if (GetJumpButton()) {
                // 进入Jump
                EnterJump();
                UpdateJump();
            }
            else {
                UpdateRun(horizontalAxis, verticalAxis, angularClampSpeed);
            }
        }
        else if (movementState == MovementState.Jump) {
            UpdateJump();
            if (controller.isGrounded) {
                // 退出Run
                movementState = MovementState.Run;
            }
        }
    }

    public void HandleJoyStick(float horizontalAxis, float verticalAxis) {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(IDLE_NAME) || stateInfo.IsName(WALK_NAME)) {
            UpdateMovement(horizontalAxis, verticalAxis);
        }
        else {
            if (animatorState == AnimatorState.Attack && movementState == MovementState.Run) {
                UpdateRun(horizontalAxis, verticalAxis, angularClampSpeedOnAttack, updatePosition:false);
            }
        }
    }

#endregion

    void UpdateAttack() {
        // var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // if (stateInfo.IsName("Attack")) {
        //     attackSeqState = AttackSeqState.Attack;
        // }
        // else {
        //     attackSeqState = AttackSeqState.Idle;
        // }
    }

    void UpdateAnimatorState() {
        if (animator.IsInTransition(0)) {
            var nextStateInfo = animator.GetNextAnimatorStateInfo(0);
            if (nextStateInfo.IsName("Idle")) {
                animatorState = AnimatorState.Idle;
            }
            else if (nextStateInfo.IsName("Attack")) {
                animatorState = AnimatorState.Attack;
            }
            else {
                if (animatorState == AnimatorState.Attack) {
                    // 不变
                }
                else {
                    animatorState = AnimatorState.Other;
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {
        animator.SetFloat("attackSpeed", attackSpeed);

        UpdateAnimatorState();

        // var horizontalAxis = Input.GetAxis("Horizontal");
        // var verticalAxis = Input.GetAxis("Vertical");
        // HandleJoyStick(horizontalAxis, verticalAxis);
        //
        // var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // if (stateInfo.IsName(IDLE_NAME) || stateInfo.IsName(WALK_NAME)) {
        //     var horizontalAxis = Input.GetAxis("Horizontal");
        //     var verticalAxis = Input.GetAxis("Vertical");
        //     UpdateMovement(horizontalAxis, verticalAxis);
        // }
    }

    private bool CanAttack() {
        // bool inAttackStrong = animator.GetCurrentAnimatorStateInfo(0).IsName("AttackStrong");
        // return !inAttackStrong;
        return true;
    }

    public void OnAttackButton() {
        if (!CanAttack()) { return; }

        animator.SetTrigger("attack");
        animator.SetInteger("attackCount", Mathf.Max(animator.GetInteger("attackCount"), 0) + 1);

        // if (attackSeqState == AttackSeqState.Attack) {
        //     animator.SetTrigger("nextAttack");
        // }
        // else {
        //     animator.ResetTrigger("nextAttack");
        // }

        // var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // if (stateInfo.IsName("Attack")) {
        //     animator.SetTrigger("nextAttack");
        // }
        // else {
        //     animator.ResetTrigger("nextAttack");
        // }
    }

#region 战斗相关

    void OnEventStartAttack(int attackType) {
        // Debug.LogFormat("OnEventStartAttack attackType:{0}", attackType);
        attackSeqCount += 1;
        attackSeqState = AttackSeqState.Attack;  // 进入攻击状态，可以触发连击

        if (mkb == null) { return; }
        mkb.OnEventStartAttack(attackType);
    }

    void OnEventEndAttack(int attackType) {
        // Debug.LogFormat("OnEventEndAttack attackType:{0}", attackType);
        attackSeqCount -= 1;
        if (attackSeqCount <= 0) {
            attackSeqState = AttackSeqState.Idle;  // 退出攻击状态，无法触发连击
        }
        else {
            attackSeqState = AttackSeqState.Attack;  // 进入攻击状态，可以触发连击
        }
        // animator.SetTrigger("attackToIdle");

        if (mkb == null) { return; }
        mkb.OnEventEndAttack(attackType);
    }

    // void OnEventExitAttack() {
    //     attackSeqState = AttackSeqState.Idle;
    //     animator.SetTrigger("attackToIdle");
    // }

#endregion

}
