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
            res = auth.CurrentUser.DisplayName;
        else
            res = "";
    }

    private void FixedUpdate()
    {
        Message.text = res;
    }

    public void SignUpClick()
    {
        if (ID is not null && PW is not null)
        {
            if (ID.text.Trim().Contains("@")) {
                signUp(ID.text.Trim(), PW.text.Trim());
                Debug.Log("SignUp : " + ID.text + " " + PW.text);
            }
            else res = "ID should have an email format.";
        }
        else
        {
            res = "Complete both email and password.";
            print(ID);
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
            else res = "ID should have an email format.";
        }
        else
        {
            res = "Complete both email and password.";
        }
        Message.text = res;
    }

    void signUp(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(
             task => {
                if (!task.IsCanceled && !task.IsFaulted)
                {
                    res = email + " : Successfully joined";
                }
                else
                {
                    res = "Fail to join";
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
                    res = email + " : Successfully logged in";
                    SceneManager.LoadScene("Main");
                }
                else
                {
                    res = "Fail to login";               
                }
            }
        );
    }
}
