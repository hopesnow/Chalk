using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErasePhysicsLine : MonoBehaviour {

    private Collider2D selectedLine;
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
    void Start ()
    {
        selectedLine = null;
        isErase = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (isErase&&selectedLine != null)
        {
            Destroy(selectedLine.gameObject);
            selectedLine = null;
        }
    }

    /** ********************************************************************************
     * @summary 線を比較する
     * タイムスタンプの新しいものに切り替える   
     ***********************************************************************************/
    private void CompareSelectedLine(Collider2D collider)
    {
        if(selectedLine != null)
        {
            //todo タイムスタンプ呼び出して比較
            float timeSelected = selectedLine.density;//酷い仮実装
            float timeCollision = collider.density;
            //今触れた線の方が新しかったら
            if(timeCollision - timeSelected > 0)
            {
                DeselectionLine(selectedLine.GetComponent<LineRenderer>());
                selectedLine = collider;
                SelectionLine(selectedLine.GetComponent<LineRenderer>());
            }
        }
        else
        {
            selectedLine = collider;
            SelectionLine(selectedLine.GetComponent<LineRenderer>());
        }
    }

    private void SelectionLine(LineRenderer line)
    {
        line.widthMultiplier = 2f;
    }

    private void DeselectionLine(LineRenderer line)
    {
        line.widthMultiplier = 1f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Line")
        {
            CompareSelectedLine(collision);
//            collision.GetComponent<LineRenderer>().widthMultiplier = 2;

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Line")
        {
            // Debug.Log("Erase Collision:" + collision.gameObject.name);
            //if(isErase)
                //Destroy(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Line")
        {
            collision.GetComponent<LineRenderer>().widthMultiplier = 1;
            if(collision == selectedLine)
            {
                selectedLine = null;
            }
        }
    }
}
