using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Auth;

public class Main : MonoBehaviour
{
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    public TMP_Text Message;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        if (user != null) {
            if (user.DisplayName.Length > 0)
                Message.text = "Hello, " + user.DisplayName + "!";
            else
                Message.text = "Hello!";
        }
        else {
            SceneManager.LoadScene("SignIn");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        Application.Quit(); // Only for Android
    }

    public void ProfileClick() {
        SceneManager.LoadScene("Profile");
    }

    public void SignOutClick() {
        auth.SignOut();
        user.DeleteAsync();
        SceneManager.LoadScene("SignIn");
    }
}
