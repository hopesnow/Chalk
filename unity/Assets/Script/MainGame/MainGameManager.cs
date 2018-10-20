using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController player;

    [SerializeField] private Text textOrigin;

    // 初期化処理
    private void Awake()
    {
    }

    // 初期化処理
    private void Start()
    {
        this.textOrigin.gameObject.SetActive(false);
        this.mainCamera.orthographicSize = 3.6f;

        // ゴール判定
        this.player.IsGoal.ObserveEveryValueChanged(x => x.Value).Distinct().Subscribe(goal =>
        {
            if (goal)
            {
                Log("Player Goal.");
            }
        });

        Log("GameStart... !");
    }

    // 更新処理
    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            this.player.Reset();
            Log("GameReset... !");
        }                                                                            
    }

    public void Log(string str)
    {
        Debug.LogFormat("{0}", str);

        this.textOrigin.gameObject.SetActive(true);

        var log = Instantiate(this.textOrigin, this.textOrigin.transform.parent);
        log.transform.SetAsLastSibling();
        log.text = str;

        this.textOrigin.gameObject.SetActive(false);
    }
}
