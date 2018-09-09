using UnityEngine;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    private void Start()
    {
        Invoke("GameStart", 2f);
    }

    private void GameStart()
    {
        SceneManager.LoadScene("MainGame");
    }
}
