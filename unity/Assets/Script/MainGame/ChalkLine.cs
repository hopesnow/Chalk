﻿using System;
using UnityEngine;

public class ChalkLine : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    private static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    private bool drawing = true;
    private long createdTime = 0;

    private Color defaultColor = Color.white;

    public bool Drawing { get { return this.drawing; } }
    public long CreatedTime { get { return this.createdTime; } }
    public LineRenderer Line { get { return this.lineRenderer; } }

    /** ********************************************************************************
     * @summary 線をひくのを完了したら呼ぶ
     ***********************************************************************************/
    public void DrawComplete()
    {
        // ２回め以降呼ばれても大丈夫なように
        if (this.Drawing)
        {
            this.drawing = false;
            this.createdTime = (long)(DateTime.Now - UnixEpoch).TotalSeconds;
        }
    }

    /** ********************************************************************************
     * @summary 線を消す
     ***********************************************************************************/
    public void DeleteLine()
    {
        Destroy(this.gameObject);
    }

    /** ********************************************************************************
     * @summary 線を選択状態にする
     ***********************************************************************************/
    public void SelectLine()
    {
        // this.lineRenderer.widthMultiplier = 2f;
        this.defaultColor = this.lineRenderer.startColor;
        this.lineRenderer.startColor = Color.red;
        this.lineRenderer.endColor = Color.red;
    }

    /** ********************************************************************************
     * @summary 線を非選択状態にする
     ***********************************************************************************/
    public void DeselectLine()
    {
        // this.lineRenderer.widthMultiplier = 1f;
        this.lineRenderer.startColor = this.defaultColor;
        this.lineRenderer.endColor = this.defaultColor;
    }
}
