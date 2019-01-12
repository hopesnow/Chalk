using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErasePhysicsLine : MonoBehaviour {

    private bool iserase;
    public bool isErase
    {
        set
        {
            this.iserase = value;
        }
        get
        {
            return this.iserase;
        }
    }

    // Use this for initialization
    void Start () {
        isErase = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isErase && collision.tag == "Line")
        {
            Debug.Log("Erase Collision:" + collision.gameObject.name);
            Destroy(collision.gameObject);
        }
    }
}
