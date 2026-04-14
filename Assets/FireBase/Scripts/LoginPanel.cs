using TMPro;
using UnityEngine;

public class LoginPanel : MonoBehaviour
{
    public TMP_InputField User;
    public TMP_InputField Password;
    void Start()
    {
        Password.contentType = TMPro.TMP_InputField.ContentType.Password;
        Password.ForceLabelUpdate();
    }
    public void Login()
    {
        FirebaseAuthManager.Instance.Login(User.text, Password.text);
    }
    public void Register()
    {
        FirebaseAuthManager.Instance.Register(User.text, Password.text);
    }
}

