using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; set; }
    public Launcher launcher;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    public void Awake()
    {
        Instance = this;
    }
    public void OnlocalGameButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
    }
    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }
    public void OnOnlineHosttButton()
    {
        launcher.StartAsHost();
        menuAnimator.SetTrigger("HostMenu");
    }
    public void OnOnlineConnectButton()
    {
        launcher.StartAsClient();
    }
    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }
    public void OnHostBackButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }
    public void OnGame()
    {
        menuAnimator.SetTrigger("InGameMenu");
    } 
}
