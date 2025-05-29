using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    // Singleton pattern chuẩn hơn
    public static GameUI Instance { get; private set; }

    [SerializeField] private Launcher launcher; // Gán trong Inspector hoặc tìm tự động
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Nếu chưa gán launcher qua Inspector, tự tìm
        if (launcher == null)
        {
            launcher = FindFirstObjectByType<Launcher>();
            if (launcher == null)
            {
                Debug.LogError("Launcher not found in scene!");
            }
        }
    }

    // Các hàm gọi launcher vẫn giữ nguyên, có thể gọi launcher từ đây

    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
    }

    public void OnOnlineGameButton()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);
        if (launcher != null)
        {
            launcher.StartAsHost();
            menuAnimator.SetTrigger("HostMenu");
        }
        else
        {
            Debug.LogError("Launcher reference is null.");
        }
    }

    public void OnOnlineConnectButton()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);
        menuAnimator.SetTrigger("HostMenu");
        if (launcher != null)
        {
            launcher.StartAsClient();
        }
        else
        {
            Debug.LogError("Launcher reference is null.");
        }
    }

    public void OnOnlineBackButton()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnGame()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);
        menuAnimator.SetTrigger("InGameMenu");
    }

    // Nếu muốn dùng addressInput để lấy địa chỉ IP hoặc server:
    public string GetAddressInput()
    {
        return addressInput != null ? addressInput.text : string.Empty;
    }
}
