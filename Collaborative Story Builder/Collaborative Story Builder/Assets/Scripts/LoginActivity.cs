using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using TMPro;
using Firebase.Firestore;
public class LoginActivity : MonoBehaviour
{
    public GameObject RegisterActivity;
    public TMP_InputField LoginEmail;
    public TMP_InputField LoginPassword;
    public Button btnLogin;
    public Button RegisterRedirect;
    public TextMeshProUGUI textError;

    private FirebaseAuth firebaseAuth;
    private UserData user;
    public FirebaseFirestore db;

    void Start()
    {
        firebaseAuth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        user = User.GetUser();

        if (user.Email != "defaultEmail")
        {
            LoginEmail.text = user.Email;
        }

        btnLogin.onClick.AddListener(LoginUser);
        RegisterRedirect.onClick.AddListener(() =>
        {
            LoginEmail.SetTextWithoutNotify("");
            LoginPassword.SetTextWithoutNotify("");

            RegisterActivity.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    public void LoginUser()
    {
        string email = LoginEmail.text.Trim();
        string password = LoginPassword.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            textError.text = "Email is required";
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            textError.text = "Password is required";
            return;
        }

        if (password.Length < 6)
        {
            textError.text = "Password must be at least 6 characters";
            return;
        }

        firebaseAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Firebase.Auth.AuthResult authResult = task.Result;
                Firebase.Auth.FirebaseUser user = authResult.User;
                string userId = user.UserId;

                textError.text = "Login successful!";

                // Fetch full user data from Firestore and save it to PlayerPrefs
                FetchUserData(userId, email);
            }
            else
            {
                textError.text = "Login failed: " + task.Exception?.Message;
            }
        });
    }

    private void FetchUserData(string userId, string email)
    {
        DocumentReference userRef = db.Collection("Users").Document(userId);

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                string username = snapshot.GetValue<string>("username");
                int userLevel = snapshot.GetValue<int>("userLevel");
                int words = snapshot.GetValue<int>("words");

                // Save data to PlayerPrefs
                PlayerPrefs.SetString("SavedUserID", userId);
                PlayerPrefs.SetString("SavedUsername", username);
                PlayerPrefs.SetString("SavedEmail", email);
                PlayerPrefs.SetInt("lvl", userLevel);
                PlayerPrefs.SetInt("words", words);
                PlayerPrefs.SetInt("login", 1);
                PlayerPrefs.Save();

                // Load Main Menu after fetching user data
                SceneManager.LoadScene("Main_Menu");
            }
            else
            {
                textError.text = "Failed to retrieve user data.";
            }
        });
    }
}
