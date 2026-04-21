using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Fusion;

public class GameUI : MonoBehaviour
{
    // Singleton pattern chuẩn hơn
    public static GameUI Instance { get; private set; }

    [SerializeField] private Launcher launcher; // Gán trong Inspector hoặc tìm tự động
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [Header("Room UI")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private TMP_Text player1Text;
    [SerializeField] private TMP_Text player2Text;
    [SerializeField] private Button startButton;
    public Button BackButton;

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
        BackButton.gameObject.SetActive(false);
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

            roomPanel.SetActive(true);
            menuAnimator.SetTrigger("HostMenu");

            player1Text.text = GameManager.Instance.username;
            player2Text.text = "Waiting...";
            startButton.interactable = false;
        }
    }

    public void OnOnlineConnectButton()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);

        if (launcher != null)
        {
            launcher.StartAsClient();

            roomPanel.SetActive(true);
            menuAnimator.SetTrigger("HostMenu");

            player1Text.text = "Connecting...";
            player2Text.text = "Connecting...";
            startButton.interactable = false;
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
        BackButton.gameObject.SetActive(true);
    }
    public void BackMainMenu()
    {
        AudioManager.Instance.PlaySFX("Click", transform.position);

        if (launcher != null && launcher.runner != null)
        {
            launcher.runner.Shutdown(); // Fusion sẽ tự gọi OnDisconnectedFromServer khi xong
            BackButton.gameObject.SetActive(false);
            menuAnimator.SetTrigger("StartMenu");
        }
        else
        {
            // Nếu chưa kết nối, quay lại menu ngay
            OnOnlineBackButton();
        }
    }
    public void UpdateRoomUI(NetworkRunner runner)
    {
        int playerCount = runner.ActivePlayers.Count();

        if (runner.IsServer)
        {
            // Host nhìn
            player1Text.text = GameManager.Instance.username;

            if (playerCount > 1)
                player2Text.text = "Opponent";
            else
                player2Text.text = "Waiting...";
        }
        else
        {
            // Client nhìn
            player1Text.text = "Host";
            player2Text.text = GameManager.Instance.username;
        }

        startButton.interactable = playerCount >= 2 && runner.IsServer;
    }

    // Nếu muốn dùng addressInput để lấy địa chỉ IP hoặc server:
    public string GetAddressInput()
    {
        return addressInput != null ? addressInput.text : string.Empty;
    }
    public void SetRoomNames(string p1, string p2)
    {
        player1Text.text = p1;
        player2Text.text = p2;
    }

    public void SetStartButton(bool canStart)
    {
        startButton.interactable = canStart;
    }
    public void OnStartGame()
    {
        if (launcher.runner.ActivePlayers.Count() < 2)
            return;

        launcher.StartMatch(); // gọi qua launcher

        menuAnimator.SetTrigger("InGameMenu");
    }
}
