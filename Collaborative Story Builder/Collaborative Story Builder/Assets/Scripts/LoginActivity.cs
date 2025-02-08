using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Extensions;

public class LoginActivity : MonoBehaviour
{
    public GameObject RegisterActivity;
    public InputField LoginEmail;
    public InputField LoginPassword;
    public Button btnLogin;
    public Button RegisterRedirect;
    public Text textError;

    private FirebaseAuth firebaseAuth;

    void Start()
    {
        firebaseAuth = FirebaseAuth.DefaultInstance;
                
        if (PlayerPrefs.HasKey("SavedEmail"))
        {
            LoginEmail.text = PlayerPrefs.GetString("SavedEmail");
        }

        btnLogin.onClick.AddListener(LoginUser);
        RegisterRedirect.onClick.AddListener(() =>
        {
            LoginEmail.SetTextWithoutNotify("");
            LoginPassword.SetTextWithoutNotify("");

            RegisterActivity.SetActive(true);
            gameObject.SetActive(false);
            //SceneManager.LoadScene("Register");

        });
    }

    private void LoginUser()
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
                textError.text = "Login successful!";
                PlayerPrefs.SetString("SavedEmail", email);
                PlayerPrefs.SetString("SavedUsername", email);
                PlayerPrefs.Save();
                SceneManager.LoadScene("Main_Menu");
            }
            else
            {
                textError.text = "Login failed: " + task.Exception?.Message;
            }
        });
    }


}
