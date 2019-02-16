using System.Collections.Generic;
using UnityEngine;

/** ********************************************************************************
 * @summary チョークの線をひく用のクラス
 ***********************************************************************************/
public class DrawPhysicsLine : MonoBehaviour
{
    [SerializeField] private ChalkLine linePrefab;
    [SerializeField] private float lineLength = 0.2f;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.white;


    private List<Vector2> linePoints = new List<Vector2>();
    private Vector3 touchPos;
    private ChalkLine newLine; // 現在引いてる線

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    private void Start()
    {
        this.linePoints = new List<Vector2>();
        this.newLine = null;
    }

    /** ********************************************************************************
     * @summary 線の初期地点を設定する
     *          基本的にはコントローラーでの判定用
     ***********************************************************************************/
    public void SetStartPos(Vector3 initPos)
    {
        this.touchPos = initPos;
        this.touchPos.z = 0;
        // ClearLines();

        // 二本め以降引くための処理
        this.linePoints.Clear();
        this.newLine = null;
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
                newLine.gameObject.name = "Line" + linePoints.Count;
            }

            LineRenderer line = newLine.GetComponent<LineRenderer>();// write line
            line.startColor = this.lineColor;
            line.endColor = this.lineColor;
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
     * @summary 線をチェックする
     ***********************************************************************************/
    public void CheckLines()
    {
        for(int i=0; i < linePoints.Count; i++)
        {
            for(int j = i+1; j < linePoints.Count; j++)
            {
                if (3 <= Mathf.Abs(j - i) && ((linePoints[i] - linePoints[j]).magnitude < lineLength * 0.5f))
                {
                    // 線として成り立っている場合
                    newLine.DrawComplete();
                    return;
                }
            }
        }

        ClearLines();
    }

    /** ********************************************************************************
     * @summary 線をチェックする
     ***********************************************************************************/
    private bool JudgeLineCross(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
    {

        var ta = (cx - dx) * (ay - cy) + (cy - dy) * (cx - ax);
        var tb = (cx - dx) * (by - cy) + (cy - dy) * (cx - bx);
        var tc = (ax - bx) * (cy - ay) + (ay - by) * (ax - cx);
        var td = (ax - bx) * (dy - ay) + (ay - by) * (ax - dx);

        return tc * td < 0 && ta * tb < 0;
        // return tc * td <= 0 && ta * tb <= 0; // 端点を含む場合
    }

    /** ********************************************************************************
     * @summary 線をクリアする
     ***********************************************************************************/
    public void ClearLines()
    {
        linePoints.Clear();
        if (newLine != null)
        {
            Destroy(newLine.gameObject);
            newLine = null;
        }
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
