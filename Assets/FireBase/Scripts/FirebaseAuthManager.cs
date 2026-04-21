using UnityEngine;
using Firebase;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance;

    private FirebaseFirestore db;
    private bool isReady = false;

    async void Start()
    {
        Instance = this;

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            db = FirebaseFirestore.DefaultInstance;
            isReady = true;

            MainThreadDispatcher.Run(() =>
            {
                if (LoginController.Instance != null)
                    LoginController.Instance.SetNotification("Firebase Ready");
            });
        }
        else
        {
            MainThreadDispatcher.Run(() =>
            {
                if (LoginController.Instance != null)
                    LoginController.Instance.SetNotification("Firebase lỗi");
            });
        }
    }

    string Hash(string input)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return System.Convert.ToBase64String(bytes);
        }
    }

    public async Task<bool> Login(string username, string password)
    {
        while (!isReady) await Task.Delay(100);

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return false;

        try
        {
            var query = await db.Collection("users")
                                .WhereEqualTo("username", username)
                                .GetSnapshotAsync();

            if (query.Count == 0)
                return false;

            var doc = query.Documents.First();

            string savedPass = doc.GetValue<string>("password");

            if (savedPass != Hash(password))
                return false;

            // ⚠️ CHỖ NGUY HIỂM → đưa về main thread
            MainThreadDispatcher.Run(() =>
            {
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();
            });

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public async Task<bool> Register(string username, string password)
    {
        while (!isReady) await Task.Delay(100);

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return false;

        try
        {
            var query = await db.Collection("users")
                                .WhereEqualTo("username", username)
                                .GetSnapshotAsync();

            if (query.Count > 0)
                return false;

            var data = new Dictionary<string, object>()
            {
                { "username", username },
                { "password", Hash(password) }
            };

            await db.Collection("users").AddAsync(data);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("username");
    }
}