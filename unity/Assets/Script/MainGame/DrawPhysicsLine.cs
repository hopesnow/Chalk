using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPhysicsLine : MonoBehaviour
{
    [SerializeField] private GameObject linePrefab;
    public float lineLength = 0.2f;
    public float lineWidth = 0.1f;
    private List<Vector2> linePoints;

    private Vector3 touchPos;
    private GameObject newLine;

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    private void Start()
    {
        linePoints = new List<Vector2>();
        newLine = null;
    }

    /** ********************************************************************************
     * @summary 更新処理
     ***********************************************************************************/
    private void Update()
    {
        // マウスでの判定
        drawLineMouse();
    }

    /** ********************************************************************************
     * @summary 線の初期地点を設定する
     *          基本的にはコントローラーでの判定用
     ***********************************************************************************/
    public void SetStartPos(Vector3 initPos)
    {
        this.touchPos = initPos;
        this.touchPos.z = 0;
        ClearLines();
    }

    /** ********************************************************************************
     * @summary 線を引く
     *          基本的にはコントローラーでの判定用
     ***********************************************************************************/
    public void DragLine(Vector3 currentPos)
    {
        Vector3 startPos = this.touchPos;
        Vector3 endPos = currentPos;
        endPos.z = 0;
        if (linePoints.Count == 0)
        {
            linePoints.Add(startPos);
        }

        // lineLength以上、カーソルが移動していたら
        if ((endPos - startPos).magnitude > lineLength)
        {
            if (newLine == null)
            {
                newLine = Instantiate(linePrefab);
                newLine.name = "Line" + linePoints.Count;
            }

            LineRenderer line = newLine.GetComponent<LineRenderer>();// write line
            line.startColor = Color.white;
            line.positionCount = 2;
            line.startWidth = lineWidth;//0.2f;

            // 線の始点、終点設定
            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);

            // 線のその他情報の設定
            Rigidbody2D rigid2D = newLine.GetComponent<Rigidbody2D>();
            rigid2D.gravityScale = 0f;
            newLine.transform.parent = this.transform;
            PolygonCollider2D polygon = newLine.GetComponent<PolygonCollider2D>();
            linePoints.Add(endPos);
            WriteLine2D(line, polygon);
            touchPos = endPos;
        }
    }

    private void drawLineMouse()
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
                if (newLine == null)
                {
                    newLine = Instantiate(linePrefab);
                    newLine.name = "Line" + linePoints.Count;
                }
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
                PolygonCollider2D polygon = newLine.GetComponent<PolygonCollider2D>();


                linePoints.Add(endPos);
                WriteLine2D(lRend, polygon);
                touchPos = endPos;
                //
            }
        }
    }

    private void WriteLine2D(LineRenderer lRend, PolygonCollider2D polygon)
    {
        lRend.positionCount = linePoints.Count;
        for (int i = 0; i + 1 <= linePoints.Count;i++)
        {
            lRend.SetPosition(i, linePoints[i]);
        }
        List<Vector2> setPointList = new List<Vector2>();
        //上の線
        for (int i = -1; i < linePoints.Count; i++)
        {
            setPointList.Add(CalcSetPoint(i, linePoints, false));
        }

        //下の線
        for (int i = linePoints.Count-1; i >= -1; i--)
        {
            setPointList.Add(CalcSetPoint(i, linePoints, true));
        }

        //コリジョン設定
        /*gameObject.GetComponent<PolygonCollider2D>()*/polygon.points = setPointList.ToArray();
    }

    /** ********************************************************************************
     * @summary 線をクリアする
     ***********************************************************************************/
    public void ClearLines()
    {
        linePoints.Clear();
        Destroy(newLine);
    }

    /** ********************************************************************************
     * @summary 設定地点を計算する
     ***********************************************************************************/
    private Vector2 CalcSetPoint(int i, List<Vector2> pointList, bool isBottomPoint)
    {
        Vector2 point1, point2;
        // Debug.Log("pointList.Count=" + pointList.Count + ", i=" + i);

        if (i <= 0)
        {
            point1 = pointList[0];
            point2 = pointList[1];
        }
        else
        {
            point1 = pointList[i - 1];
            point2 = pointList[i];
        }

        //前回の点、今回の点の2点間の角度を求め、
        //さらそこに90度加算することで、
        //2点間にかかる線分に下りる垂線と平行にになる線(前回の点から線を引いて)の角度を求める
        float aim = GetAim(point2, point1);
        float correctionAim = 90;

        if (isBottomPoint)
        {
            correctionAim *= -1;
        }

        if (point1.y < point2.y)
        {
            aim = GetAim(point1, point2);
            correctionAim *= -1;
        }

        aim += correctionAim;

        //前回の点を角度の方向に移動
        Vector2 setPoint = Vector2.zero;
        if (i == -1)
        {
            point2 = point1;
        }

        setPoint.x = (float)(point2.x + lineWidth * 0.5f * Mathf.Cos(Mathf.Deg2Rad * aim));
        setPoint.y = (float)(point2.y + lineWidth * 0.5f * Mathf.Sin(Mathf.Deg2Rad * aim));

        return setPoint;
    }

    /** ********************************************************************************
     * @summary p2からp1への角度を求める
     ***********************************************************************************/
    private float GetAim(Vector2 p1, Vector2 p2)
    {
        Vector2 a = new Vector2(1, 0);
        Vector2 b = p2 - p1;
        return Vector2.Angle(a, b);
    }

}
