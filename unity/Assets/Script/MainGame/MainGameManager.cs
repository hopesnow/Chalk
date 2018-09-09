using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        this.mainCamera.orthographicSize = 3.6f;
    }
}
