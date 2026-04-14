using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private bool isReady = false;

    async void Start()
    {
        Instance = this;

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            isReady = true;
            Debug.Log("Firebase Ready");
        }
        else
        {
            Debug.LogError("Firebase lỗi: " + dependencyStatus);
        }
    }

    // ================= REGISTER =================
    public async void Register(string email, string password, string username)
    {
        if (!isReady)
        {
            Debug.LogError("Firebase chưa sẵn sàng!");
            return;
        }

        try
        {
            //  Check username
            var querySnapshot = await db.Collection("users")
                                        .WhereEqualTo("username", username)
                                        .GetSnapshotAsync();

            if (querySnapshot.Count > 0)
            {
                Debug.LogError("Username đã tồn tại!");
                return;
            }

            //  Tạo account
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser newUser = result.User;

            //  Lưu Firestore
            var userData = new Dictionary<string, object>()
            {
                { "username", username },
                { "email", email }
            };

            await db.Collection("users")
                    .Document(newUser.UserId)
                    .SetAsync(userData);

            Debug.Log("Đăng ký thành công!");
            LoginController.Instance.LoginPanel();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Register lỗi: " + e);
        }
    }

    // ================= LOGIN =================
    public async void Login(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            Debug.Log("Đăng nhập thành công!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Login lỗi: " + e);
        }
    }

    // ================= LOGOUT =================
    public void Logout()
    {
        auth.SignOut();
        Debug.Log("Đã đăng xuất");
    }
}