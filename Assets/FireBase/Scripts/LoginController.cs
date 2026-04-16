using TMPro;
using UnityEngine;

public class LoginController : MonoBehaviour
{
    public static LoginController Instance;
    public TMP_Text Notification; // thông báo 
    public TMP_InputField Name; // khi Register
    public TMP_InputField User;
    public TMP_InputField Password;
    public GameObject loadingUI;
    private Animator animator;
    public void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        Password.ForceLabelUpdate();
        Notification.text = "";
    }
    public void SetNotification(string message)
    {
        Notification.text = message;
    }
    public async void Login()
    {
        loadingUI.SetActive(true);
        await FirebaseAuthManager.Instance.Login(User.text, Password.text);
        loadingUI.SetActive(false);
    }
    public async void Register()
    {
        loadingUI.SetActive(true);
        await FirebaseAuthManager.Instance.Register(User.text, Password.text, Name.text);
        loadingUI.SetActive(false);
    }
    public void RegisterPanel()
    {
        animator.SetBool("Register", true);
    }
    public void LoginPanel()
    {
        animator.SetBool("Register", false);
    }
  
}

