using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErasePhysicsLine : MonoBehaviour {

    [SerializeField]
    private List<Collider2D> lineList;
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
        lineList = new List<Collider2D>();
        selectedLine = null;
        isErase = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
    }

    /** ********************************************************************************
     * @summary 線を比較する
     * lineListの中で一番新しいもの   
     * タイムスタンプの新しいものにselectedLineを切り替える   
     ***********************************************************************************/
    private void CompareSelectedLine()
    {
        if(lineList.Count == 0)
        {
            selectedLine = null;
            return;
        }
        long timeSelected = lineList[0].GetComponent<ChalkLine>().CreatedTime;
        int num = 0;
        for (int i = 0; i < lineList.Count; i++)
        {
            long timeCollision = lineList[i].GetComponent<ChalkLine>().CreatedTime;
            if (timeCollision - timeSelected > 0)
            {
                num = i;
            }
        }
        if (selectedLine != null)
        {
            if (lineList[num] != selectedLine)
                DeselectionLine(selectedLine.GetComponent<LineRenderer>());
        }
        selectedLine = lineList[num];
        SelectionLine(selectedLine.GetComponent<LineRenderer>());
    }

    private void SelectionLine(LineRenderer line)
    {
        line.widthMultiplier = 2f;
    }

    private void DeselectionLine(LineRenderer line)
    {
        line.widthMultiplier = 1f;
    }

    /** ********************************************************************************
     * @summary 線を比較する
     * lineListの中で一番新しいもの   
     * タイムスタンプの新しいものにselectedLineを切り替える   
     ***********************************************************************************/
    public void ClearLines()
    {
        lineList = new List<Collider2D>();
        selectedLine = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Line")
        {
            lineList.Add(collision);
            CompareSelectedLine();
            // collision.GetComponent<LineRenderer>().widthMultiplier = 2;

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Line")
        {
            // Debug.Log("Erase Collision:" + collision.gameObject.name);
            if (isErase && selectedLine != null)
            {
                if (!selectedLine.GetComponent<ChalkLine>().Drawing)
                {
                    DeselectionLine(selectedLine.GetComponent<LineRenderer>());
                    lineList.Remove(selectedLine);
                    CompareSelectedLine();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Line")
        {
            DeselectionLine(collision.GetComponent<LineRenderer>());
            lineList.Remove(collision);
            CompareSelectedLine();
            if(collision == selectedLine)
            {
                selectedLine = null;
            }
        }
    }
}
