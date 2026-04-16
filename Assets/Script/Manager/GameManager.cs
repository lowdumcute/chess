using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Info player")]
    public string username;

    private void Awake()
    {
        //  Nếu đã có instance thì xoá cái mới
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 🔥 Không bị destroy khi chuyển scene
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateUI()
    {
        username = PlayerPrefs.GetString("username", "Guest");
        Debug.Log("Username: " + username);
    }
}