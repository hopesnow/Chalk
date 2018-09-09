using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawPhysicsLine : MonoBehaviour {


    public GameObject linePrefab;
    public float lineLength = 0.2f;
    public float lineWidth = 0.1f;
    private List<Vector3> linePoints;
    private List<GameObject> lines;

    private Vector3 touchPos;

    void Start()
    {
        linePoints = new List<Vector3>();
        lines = new List<GameObject>();
    }

    void Update()
    {
        drawLine();
    }

    void drawLine()
    {

        if (Input.GetMouseButtonDown(0))
        {
            touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 0;
            ClearLines();
        }

        if (Input.GetMouseButton(0))
        {

            Vector3 startPos = touchPos;
            if(linePoints.Count==0)
                linePoints.Add(startPos);
            Vector3 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            endPos.z = 0;

            if ((endPos - startPos).magnitude > lineLength)
            {
                /*
                GameObject obj = Instantiate(linePrefab, transform.position, transform.rotation) as GameObject;
                obj.transform.position = (startPos + endPos) / 2;
                obj.transform.right = (endPos - startPos).normalized;

                obj.transform.localScale = new Vector3((endPos - startPos).magnitude, lineWidth, lineWidth);

                obj.transform.parent = this.transform;

                touchPos = endPos;
                */
                //test

                //GameObject newLine = new GameObject("Line"+linePoints.Count);
                GameObject newLine = Instantiate(linePrefab);
                newLine.name = "Line" + linePoints.Count;
                LineRenderer lRend = newLine.GetComponent<LineRenderer>();// write line
                lRend.startColor = Color.white;
                lRend.positionCount = 2;
                lRend.startWidth = lineWidth;//0.2f;
                Vector3 startVec = startPos;
                Vector3 endVec = endPos;
                lRend.SetPosition(0, startVec);
                lRend.SetPosition(1, endVec);
                Rigidbody2D rigid2D = newLine.GetComponent<Rigidbody2D>();
                rigid2D.gravityScale = 0f;
                newLine.transform.parent = this.transform;
                CapsuleCollider2D capsule = newLine.GetComponent<CapsuleCollider2D>();// set collision
                //capsule.radius = lineWidth / 2;
                capsule.size = new Vector2(lineLength/2, (endPos - startPos).magnitude);
                //capsule.center = Vector3.zero;
                capsule.offset = Vector2.zero;
                //capsule.direction = CapsuleDirection2D.Vertical;//2; // Z-axis for easier "LookAt" orientation
                capsule.transform.position = startPos + (endPos - startPos) / 2;
                capsule.transform.LookAt(startPos);
                //capsule.height = (endPos - startPos).magnitude;

                lines.Add(newLine);
                linePoints.Add(endPos);
                touchPos = endPos;
                //
            }
        }
    }

    void WriteLine2D()
    {
        for (int i = 0; i + 1 <= linePoints.Count;i++)
        {
            
        }

    }

    void ClearLines()
    {
        for (int i = 0; i < lines.Count; i++)// Clear Object
        {
            Destroy(lines[i]);
        }
        linePoints.Clear();// Clear List
        lines.Clear();
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
        Debug.Log("enter2d");
	}

	private void OnCollisionEnter(Collision collision)
	{
        Debug.Log("enter");
	}
}
