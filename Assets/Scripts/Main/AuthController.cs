using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;

public class User {
    public string email;
    public string name;

    public User() {
    }

    public User(string email, string name) {
        this.email = email;
        this.name = name;
    }
}

public class AuthController : MonoBehaviour
{
    public TMP_InputField ID;
    public TMP_InputField PW;
    public TMP_Text Message;
    string res;
    FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
            SceneManager.LoadScene("Main");
        else
            Message.text = "";
    }

    public void SignUpMoveClick()
    {
        SceneManager.LoadScene("SignUp");
    }

    public void SignInMoveClick()
    {
        SceneManager.LoadScene("SignIn");
    }

    public void SignUpClick()
    {
        if (ID is not null && PW is not null)
        {
            if (ID.text.Trim().Contains("@")) {
                signUp(ID.text.Trim(), PW.text.Trim());
                Debug.Log("SignUp : " + ID.text + " " + PW.text);
            }
            else Message.text = "ID should have an email format.";
        }
        else
        {
            Message.text = "Complete both email and password.";
        }
        
    }

    public void SignInClick()
    {
        if (ID is not null && PW is not null)
        {
            if (ID.text.Trim().Contains("@")) {
                signIn(ID.text.Trim(), PW.text.Trim());
                Debug.Log("SignIn : " + ID.text + " " + PW.text);
            }
            else Message.text = "ID should have an email format.";
        }
        else
        {
            Message.text = "Complete both email and password.";
        }
    }

    void signUp(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(
             task => {
                if (!task.IsCanceled && !task.IsFaulted)
                {
                    Message.text = email + " : Successfully joined";
                }
                else
                {
                    Message.text = "Fail to join";
                    return;
                }
             }
         );
    }

    void signIn(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(
            task => {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    Message.text = email + " : Successfully logged in";
                    SceneManager.LoadScene("Main");
                }
                else
                {
                    Message.text = "Login failed. Check your email and password again.";               
                }
            }
        );
    }
}
