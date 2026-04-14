using TMPro;
using UnityEngine;

public class LoginController : MonoBehaviour
{
    public static LoginController Instance;
    public TMP_InputField Name; // khi Register
    public TMP_InputField User;
    public TMP_InputField Password;
    private Animator animator;
    public void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        Password.ForceLabelUpdate();
    }
    public void Login()
    {
        FirebaseAuthManager.Instance.Login(User.text, Password.text);
    }
    public void Register()
    {
        FirebaseAuthManager.Instance.Register(User.text, Password.text, Name.text);
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

