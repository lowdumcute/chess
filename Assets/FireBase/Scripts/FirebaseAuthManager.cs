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
    public async Task Register(string email, string password, string username)
    {       
        if (!isReady)
        {
            LoginController.Instance.SetNotification("Firebase chưa sẵn sàng");
            return;
        }

        if (string.IsNullOrEmpty(username))
        {
            LoginController.Instance.SetNotification("Vui lòng nhập username");
            return;
        }

        try
        {
            var querySnapshot = await db.Collection("users")
                                        .WhereEqualTo("username", username)
                                        .GetSnapshotAsync();

            if (querySnapshot.Count > 0)
            {
                LoginController.Instance.SetNotification("Username đã tồn tại");
                return;
            }

            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

            if (result == null || result.User == null)
            {
                LoginController.Instance.SetNotification("Đăng ký thất bại");
                return;
            }

            FirebaseUser newUser = result.User;

            var userData = new Dictionary<string, object>()
            {
                { "username", username },
                { "email", email }
            };

            await db.Collection("users")
                    .Document(newUser.UserId)
                    .SetAsync(userData);

            LoginController.Instance.SetNotification("Đăng ký thành công!");
            LoginController.Instance.LoginPanel();
        }
        catch (FirebaseException e)
        {
            HandleAuthError(e);
        }
        catch (System.Exception e)
        {
            LoginController.Instance.SetNotification("Lỗi hệ thống");
        }
    }

    // ================= LOGIN =================
    public async Task Login(string email, string password)
    {
        while (!isReady)    
        {
            await Task.Delay(100);
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LoginController.Instance.SetNotification("Nhập email và password");
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);

            if (result == null || result.User == null)
            {
                LoginController.Instance.SetNotification("Đăng nhập thất bại");
                return;
            }

            FirebaseUser user = result.User;

            await LoadUserData(user.UserId);

            LoginController.Instance.SetNotification("");

            OnLoginSuccess();
        }
        catch (FirebaseException e)
        {
            HandleAuthError(e);
        }
        catch (System.Exception)
        {
            LoginController.Instance.SetNotification("Lỗi hệ thống");
        }
    }
    void OnLoginSuccess()
    {
        if (LoginController.Instance != null)
            LoginController.Instance.loadingUI.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateUI();

        if (SenceController.Instance != null)
        {
            SenceController.Instance.StartCoroutine(SenceController.Instance.ChangeSence("Mainmenu") );
        }
            
    }
    private async Task LoadUserData(string userId)
    {
        try
        {
            DocumentSnapshot snapshot = await db.Collection("users")
                                                .Document(userId)
                                                .GetSnapshotAsync();

            if (snapshot.Exists)
            {
                string username = snapshot.GetValue<string>("username");

                // 👉 Lưu lại để dùng trong game
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();

                // 👉 hoặc dùng cho Photon
                // PhotonNetwork.NickName = username;
            }
            else
            {
                Debug.LogError("Không tìm thấy user data!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi load user: " + e);
        }
    }

    // ================= LOGOUT =================
    public void Logout()
    {
        auth.SignOut();
        Debug.Log("Đã đăng xuất");
    }
    void HandleAuthError(FirebaseException e)
    {
        AuthError errorCode = (AuthError)e.ErrorCode;

        switch (errorCode)
        {
            case AuthError.EmailAlreadyInUse:
                LoginController.Instance.SetNotification("Email đã tồn tại");
                break;

            case AuthError.InvalidEmail:
                LoginController.Instance.SetNotification("Email không hợp lệ");
                break;

            case AuthError.WeakPassword:
                LoginController.Instance.SetNotification("Password phải >= 6 ký tự");
                break;

            case AuthError.WrongPassword:
                LoginController.Instance.SetNotification("Sai mật khẩu");
                break;

            case AuthError.UserNotFound:
                LoginController.Instance.SetNotification("Tài khoản không tồn tại");
                break;

            default:
                LoginController.Instance.SetNotification("Lỗi: " + errorCode.ToString());
                break;
        }
    }
}