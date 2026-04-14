using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance;
    private FirebaseAuth auth;

    void Start()
    {
        Instance = this;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Ready");
            }
            else
            {
                Debug.LogError("Firebase lỗi: " + task.Result);
            }
        });
    }

    // ĐĂNG KÝ
    public void Register(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Đăng ký lỗi");
                return;
            }

            Debug.Log("Đăng ký thành công!");
        });
    }

    // ĐĂNG NHẬP
    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Đăng nhập thất bại");
                return;
            }

            Debug.Log("Đăng nhập thành công!");
        });
    }

    // ĐĂNG XUẤT
    public void Logout()
    {
        auth.SignOut();
        Debug.Log("Đã đăng xuất");
    }
}