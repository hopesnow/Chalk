using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController player;

    // 初期化処理
    private void Start()
    {
        this.mainCamera.orthographicSize = 3.6f;
    }

    // 更新処理
    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            this.player.Reset();
        }
    }
}
