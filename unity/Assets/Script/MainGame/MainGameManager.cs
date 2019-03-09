using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController[] players;

    [SerializeField] private Transform stageParent;
    [SerializeField] private GameObject[] stages;

    [SerializeField] private Text textOrigin;

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    private void Start()
    {
        PlayerController[] test = FindObjectsOfType<PlayerController>();
        this.players = test;

        this.textOrigin.gameObject.SetActive(false);
        this.mainCamera.orthographicSize = 3.6f;

        foreach (var player in this.players)
        {
            // ゴール判定オブザーバーの登録
            player.IsGoal.ObserveEveryValueChanged(x => x.Value).Subscribe(goal =>
            {
                if (goal)
                {
                    Log(string.Format("Player{0} Goal.", player.PlayerNo + 1));
                }
            });
        }

        Log("GameStart... !");
        Log("ESCキーでリセット");
        Log("ELECOM DIRECT INPUT ONLY NOW.");
    }

    /** ********************************************************************************
     * @summary 更新処理
     ***********************************************************************************/
    private void Update()
    {
        if (Input.GetButtonDown("DebugReset"))
        {
            foreach (var player in this.players)
            {
                player.Reset();
            }

            Log("GameReset... !");
        }

        if (Input.GetButtonDown("Pause"))
        {
            Debug.Log("Pause");
        }
    }

    /** ********************************************************************************
     * @summary ログ出力処理
     ***********************************************************************************/
    public void Log(string str)
    {
        Debug.LogFormat("{0}", str);

        this.textOrigin.gameObject.SetActive(true);

        var log = Instantiate(this.textOrigin, this.textOrigin.transform.parent);
        log.transform.SetAsFirstSibling();
        log.text = str;

        this.textOrigin.gameObject.SetActive(false);
    }

    /** ********************************************************************************
     * @summary ステージ選択処理(number: 0から)範囲外だとランダムになる
     ***********************************************************************************/
    public void SetStage(int number = -1)
    {
        // 前回ステージを削除
        foreach (Transform stageChild in this.stageParent)
        {
            Destroy(stageChild.gameObject);
        }

        // 範囲外チェック
        if (number < 0 || number > this.stages.Length - 1)
        {
            // ランダムに選ぶ
            number = Random.Range(0, this.stages.Length);   // stages.Lengthは含まない(intだから)
        }

        // ステージ生成
        Instantiate(this.stages[number], this.stageParent);
    }
}
