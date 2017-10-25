using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MKBCollider : MonoBehaviour {

    public bool canHit = false;
    public int attackType = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    void OnTriggerEnter(Collider other) {
        // Debug.LogFormat("other:{0}, canHit:{1}", other, canHit);
        if (!canHit) { return; }
        // Debug.Log(other);
        // other.SendMessage("OnHit", 2.5f, SendMessageOptions.DontRequireReceiver);
        if (attackType == 0) {
            other.SendMessage("OnHit", 2.5f, SendMessageOptions.DontRequireReceiver);

        }
        else {
            other.SendMessage("OnStrongHit", 3.0f, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnEventStartAttack(int attackType_) {
        canHit = true;
        attackType = attackType_;
    }

    public void OnEventEndAttack(int attackType_) {
        canHit = false;
        attackType = attackType_;
    }

    // 待办：需要处理打断
    // public void CancelAttack() {
    //     canHit = false;
    // }
}
