using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            MainThreadDispatcher.Run(() =>
            {
                LoginController.Instance?.SetNotification("Firebase Ready");
            });
        }
        else
        {
            MainThreadDispatcher.Run(() =>
            {
                LoginController.Instance?.SetNotification("Firebase lỗi");
            });
        }
    }

    // ================= REGISTER =================
    public async Task<bool> Register(string email, string password, string username)
    {
        while (!isReady) await Task.Delay(100);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
            return false;

        try
        {
            // check username trùng
            var check = await db.Collection("users")
                                .WhereEqualTo("username", username)
                                .GetSnapshotAsync();

            if (check.Count > 0)
                return false;

            // tạo account auth
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

            if (result == null || result.User == null)
                return false;

            string uid = result.User.UserId;

            // lưu firestore
            var data = new Dictionary<string, object>()
            {
                { "username", username },
                { "email", email }
            };

            await db.Collection("users").Document(uid).SetAsync(data);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    // ================= LOGIN =================
    public async Task<bool> Login(string email, string password)
    {
        while (!isReady) await Task.Delay(100);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return false;

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);

            if (result == null || result.User == null)
                return false;

            string uid = result.User.UserId;

            // load username từ Firestore
            var snapshot = await db.Collection("users")
                                   .Document(uid)
                                   .GetSnapshotAsync();

            if (!snapshot.Exists)
                return false;

            string username = snapshot.GetValue<string>("username");

            // ⚠️ đưa về main thread
            MainThreadDispatcher.Run(() =>
            {
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();
            });

            return true;
        }
        catch (FirebaseException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public void Logout()
    {
        auth.SignOut();
        PlayerPrefs.DeleteKey("username");
    }
}