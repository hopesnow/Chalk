using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    [SerializeField] private Button startButton;

    private void Start()
    {
        // Invoke("GameStart", 2f);
        this.startButton.onClick.AddListener(() =>
        {
            GameStart();
        });
    }

    private void GameStart()
    {
        SceneManager.LoadScene("MainGame");
    }
}
