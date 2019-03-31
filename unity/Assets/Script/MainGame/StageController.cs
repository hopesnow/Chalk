using System.Linq;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField] private Transform[] startObj = new Transform[4];
    [SerializeField] private Transform[] goalObj = new Transform[4];

    [SerializeField] private Transform zoomObj;
    [SerializeField] private SpriteRenderer goalSprite;
    [SerializeField] private BoxCollider2D goalArea;
    [SerializeField] private float zoomSize;

    public Vector3[] StartPos { get { return this.startObj.Select(l => l.position).ToArray(); } }
    public Vector3[] GoalPos { get { return this.goalObj.Select(l => l.position).ToArray(); } }
    public Vector3 ZoomPos { get { return this.zoomObj.position; } }
    public SpriteRenderer GoalSprite { get{ return this.goalSprite; } }
    public float ZoomSize { get { return this.zoomSize; } }

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    public void Init()
    {
        this.goalSprite.color = new Color(1f, 1f, 1f, 0f);
    }

    /** ********************************************************************************
     * @summary ゴール判定消滅の処理
     ***********************************************************************************/
    public void DisableGoalArea()
    {
        goalArea.enabled = false;
    }

    /** ********************************************************************************
     * @summary ゴール後のリセット処理
     ***********************************************************************************/
    public void Reset()
    {
        this.goalSprite.color = new Color(1f, 1f, 1f, 0f);
        goalArea.enabled = true;
    }
}
