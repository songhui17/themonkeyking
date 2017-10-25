using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
    public Animator animator;
    public Rigidbody rigidbody;
    
    public LayerMask groundLayer = -1;

    // 单位属性
    [Header("Unit Properties")]
    public float maxHp = 10;
    public float hp = 10;

    // 固定力
    [Header("Constant Force")]
    public Vector3 constantForce;

    void Start() {
    }

    // // 需要处理击飞
    // void UpdateHitFly() {
    // }
    void OnCollisionEnter(Collision collision) {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // Debug.LogFormat("OnCollisionEnter collision.gameObject:{0}, layer:{1}, groundLayer:{2}", collision.gameObject, collision.gameObject.layer, groundLayer.value);
        // Debug.LogFormat("1<<layer:{0}, &:{1}", 1 << collision.gameObject.layer, (1 << collision.gameObject.layer) & groundLayer);
        bool isTargetLayer = ((1 << collision.gameObject.layer) & groundLayer) != 0;
        if (isTargetLayer && stateInfo.IsName("HitFly")) {
            if (!IsDead()) {
                animator.SetTrigger("idle");
            }
        }
    }

    void Update() {

    }

    private bool IsDead() {
        return hp <= 0;
    }

    private bool CanBeHit() {
        return !IsDead();
    }

    private void ApplyDamage(float damage) {
        hp -= damage;
        animator.SetBool("isDead", IsDead());
    }

    public void OnHit(float damage=2.5f) {
        if (!CanBeHit()) { return; }

        ApplyDamage(damage);
        if (IsDead()) {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die")) {
                animator.SetTrigger("die");
            }
        }
        else {
            animator.SetTrigger("hit");
        }
    }

    public void OnStrongHit(float damage=3.0f) {
        if (!CanBeHit()) { return; }

        ApplyDamage(damage);
        HitFly();
    }

    public void AddForce(Vector3 force) {
        if (rigidbody == null) { return; }
        rigidbody.AddForce(force);
    }

    public Vector3 GetConstantForce() {
        return constantForce;
    }

    [ContextMenu("HitFly")]
    public void HitFly() {
        animator.SetTrigger("hitFly");
        AddForce(GetConstantForce());
    }
}
