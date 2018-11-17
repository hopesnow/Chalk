using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController[] players;

    [SerializeField] private Text textOrigin;

    // 初期化処理
    private void Awake()
    {
    }

    // 初期化処理
    private void Start()
    {
        PlayerController[] test = FindObjectsOfType<PlayerController>();
        players = test;
        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];
            this.textOrigin.gameObject.SetActive(false);
            this.mainCamera.orthographicSize = 3.6f;

            // ゴール判定
            player.IsGoal.ObserveEveryValueChanged(x => x.Value).Subscribe(goal =>
            {
                if (goal)
                {
                    Log(string.Format("Player{0} Goal.", player.GetPlayerNo()+1));
                }
            });
        }

        Log("GameStart... !");
        Log("Fキーでリセット");
    }

    // 更新処理
    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            for (int i = 0; i < players.Length; i++)
            {
                PlayerController player = players[i];
                player.Reset();
                Log("GameReset... !");
            }
        }                                                                            
    }

    public void Log(string str)
    {
        Debug.LogFormat("{0}", str);

        this.textOrigin.gameObject.SetActive(true);

        var log = Instantiate(this.textOrigin, this.textOrigin.transform.parent);
        log.transform.SetAsFirstSibling();
        log.text = str;

        this.textOrigin.gameObject.SetActive(false);
    }
}
