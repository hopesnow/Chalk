using UnityEngine;

public class GraffitiManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    // 初期化処理
    private void Awake()
    {
        EnvironmentManager.LoadCompleteEvent += LoadCompleted;
        EnvironmentManager.CreateInstance();
    }

    // 初期化処理
    private void LoadCompleted()
    {
        // 新しいカメラを設定して、古いカメラを削除する
        var oldCamera = this.canvas.worldCamera;
        this.canvas.worldCamera = EnvironmentManager.Instance.UiCamera;
        if (oldCamera != null)
        {
            Destroy(oldCamera);
        }
    }
}
