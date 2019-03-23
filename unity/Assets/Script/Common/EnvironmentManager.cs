using System;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public const string ResourceName = "EnvironmentManager";

    [SerializeField] private Camera uiCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera debugCamera;

    public static Action LoadCompleteEvent;

    public Camera UiCamera { get { return this.uiCamera; } }

    // シングルトン
    static private EnvironmentManager instance;
    static public EnvironmentManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<EnvironmentManager>(ResourceName);
            }
            return instance;
        }
    }

    // シングルトン生成
    static public void CreateInstance()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<EnvironmentManager>(ResourceName));
            if (LoadCompleteEvent != null)
            {
                LoadCompleteEvent();
            }
        }
        else
        {
            Debug.LogWarning("EnvironmentManager's instance is exist already.");
        }
    }

	// 初期化処理
	private void Awake()
    {
        Destroy(this.debugCamera.gameObject);
        DontDestroyOnLoad(this.gameObject);
	}
}
