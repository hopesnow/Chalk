using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ErasePhysicsLine : MonoBehaviour
{
    private Dictionary<int, ChalkLine> selectableLines = new Dictionary<int, ChalkLine>();
    private ChalkLine selectedLine;

    public bool IsErasable { get { return this.selectedLine != null; } }    // 消すオブジェクトがあるかどうか(消せるかどうか)

    private void Update()
    {
        if (this.selectableLines.Count() > 0)
        {
            var log = new StringBuilder();
            foreach (var lines in this.selectableLines)
            {
                log.AppendFormat("color: {0} / createdTime: {1}\n", lines.Value.Line.startColor, lines.Value.CreatedTime);
            }

            Debug.Log(log.ToString());
        }
    }

    /** ********************************************************************************
     * @summary 選択状態の線を削除する
     ***********************************************************************************/
    public void DeleteLine()
    {
        if (this.selectedLine != null)
        {
            var removeId = this.selectedLine.GetInstanceID();
            this.selectedLine.DeleteLine();
            this.selectedLine = null;
            this.selectableLines.Remove(removeId);

            // 消した後に次のやつを選択する
            if (this.selectableLines.Count() > 0)
            {
                this.selectedLine = this.selectableLines.OrderByDescending(item => item.Value.CreatedTime).First().Value;
                this.selectedLine.SelectLine();
            }
        }
    }

    /** ********************************************************************************
     * @summary 消す想定のラインを選択する
     ***********************************************************************************/
    private void CheckSelect(ChalkLine line)
    {
        var id = line.GetInstanceID();

        // 既にあればスキップする
        if (this.selectableLines.ContainsKey(id))
        {
            return;
        }

        // まだ書いてる途中のときスキップする
        if (line.Drawing)
        {
            return;
        }

        // 選択中のものを変更する
        // Debug.Log();
        this.selectableLines.Add(id, line);
        if (this.selectedLine != null)
        {
            this.selectedLine.DeselectLine();
        }

        this.selectedLine = this.selectableLines.OrderByDescending(item => item.Value.CreatedTime).First().Value;
        this.selectedLine.SelectLine();
    }

    /** ********************************************************************************
     * @summary 入ったときの処理
     ***********************************************************************************/
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Line")
        {
            // 選択状態の更新
            var line = collision.GetComponent<ChalkLine>();
            CheckSelect(line);
        }
    }

    /** ********************************************************************************
     * @summary 滞在時の処理
     ***********************************************************************************/
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Line")
        {
            // 選択状態の更新
            var line = collision.GetComponent<ChalkLine>();
            CheckSelect(line);
        }
    }

    /** ********************************************************************************
     * @summary 出たときの処理
     ***********************************************************************************/
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Line")
        {
            var removeId = collision.GetComponent<ChalkLine>().GetInstanceID();
            this.selectableLines.Remove(removeId);

            if (this.selectedLine != null && this.selectedLine.GetInstanceID() == removeId)
            {
                // 選択中のものがあれば一旦非選択状態にする
                if (this.selectedLine != null)
                {
                    this.selectedLine.DeselectLine();
                }

                // 選択中のものを変える
                if (this.selectableLines.Count() > 0)
                {
                    this.selectedLine = this.selectableLines.OrderByDescending(item => item.Value.CreatedTime).First().Value;
                    this.selectedLine.SelectLine();
                }
                else
                {
                    this.selectedLine = null;
                }
            }
        }
    }
}
