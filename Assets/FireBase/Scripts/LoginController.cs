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

        bool success = await FirebaseAuthManager.Instance.Login(User.text, Password.text);

        // 🔥 QUAY VỀ MAIN THREAD
        MainThreadDispatcher.Run(() =>
        {
            loadingUI.SetActive(false);

            if (success)
            {
                SetNotification("");

                SenceController.Instance.StartCoroutine(
                    SenceController.Instance.ChangeSence("Mainmenu")
                );
            }
            else
            {
                SetNotification("Sai tài khoản hoặc mật khẩu");
            }
        });
    }

    public async void Register()
    {
        loadingUI.SetActive(true);

        bool success = await FirebaseAuthManager.Instance.Register(Name.text, Password.text);

        MainThreadDispatcher.Run(() =>
        {
            loadingUI.SetActive(false);

            if (success)
            {
                SetNotification("Đăng ký thành công!");
                LoginPanel();
            }
            else
            {
                SetNotification("Username đã tồn tại");
            }
        });
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

