using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using TMPro;
public class RegisterActivity : MonoBehaviour
{
    public GameObject LoginActivity;
    public TMP_InputField RegisterEmail;
    public TMP_InputField RegisterPassword;
    public TMP_InputField ConfirmPassword;
    public Button btnRegister;
    public Button LoginRedirect;
    public TextMeshProUGUI textError;

    private FirebaseAuth firebaseAuth;

    void Start()
    {
        PlayerPrefs.DeleteAll();
        // Initialize Firebase Auth
        firebaseAuth = FirebaseAuth.DefaultInstance;

        if (PlayerPrefs.HasKey("SavedEmail"))
        {
            SceneManager.LoadScene("Main_Menu");
        }

        btnRegister.onClick.AddListener(RegisterUser);
        LoginRedirect.onClick.AddListener(() => {
            LoadLogin();
            //SceneManager.LoadScene("Login");
            });
        NotificationManager.Instance.Notify("Welcome to CSB! Enter your information to start.", 3f);
    }

    private void RegisterUser()
    {
        string email = RegisterEmail.text.Trim();
        string password = RegisterPassword.text.Trim();
        string confirmPassword = ConfirmPassword.text.Trim();

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

        if (password != confirmPassword)
        {
            textError.text = "Passwords do not match";
            return;
        }

    firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
        {
            textError.text = "Registration successful!";
            LoadLogin();
            //SceneManager.LoadScene("Login");
        }
        else
        {
            textError.text = "Registration failed: " + task.Exception?.Message;
        }
    });

    }
    public void LoadLogin()
    {
        RegisterEmail.SetTextWithoutNotify("");
        RegisterPassword.SetTextWithoutNotify("");
        ConfirmPassword.SetTextWithoutNotify("");
        LoginActivity.SetActive(true);
        gameObject.SetActive(false);
    }
}
