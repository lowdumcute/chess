using UnityEngine;

public class SetCamera : MonoBehaviour
{
    public static SetCamera Instance { get; set; }
    public GameObject mainCamera;
    void Awake()
    {
        Instance = this;
    }
    public void SetCameraClient()
    {
        gameObject.transform.position = new Vector3(0, 13, 10);
        gameObject.transform.rotation = Quaternion.Euler(130, 0, 180);
    }
}
