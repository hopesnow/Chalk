using System;
using UnityEngine;

public class ChalkLine : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    private static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    private bool drawing = true;
    private long createdTime = 0;

    public bool Drawing { get { return this.drawing; } }
    public long CreatedTime { get { return this.createdTime; } }

    // 線をひくのを完了したら呼ぶ
    public void DrawComplete()
    {
        this.drawing = false;
        this.createdTime = (long)(DateTime.Now - UnixEpoch).TotalSeconds;
    }

    // 線を消す
    public void DeleteLine()
    {
        Destroy(this.gameObject);
    }
}
