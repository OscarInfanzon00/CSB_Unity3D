using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using TMPro;
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

    void Start()
    {
        firebaseAuth = FirebaseAuth.DefaultInstance;
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
                    Firebase.Auth.AuthResult authResult = task.Result;
                    Firebase.Auth.FirebaseUser user = authResult.User;
                    string userId = user.UserId;

                    textError.text = "Login successful!";

                    User.SaveUser(userId, email, email, 0, 0);

                    PlayerPrefs.SetInt("login", 1);
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
