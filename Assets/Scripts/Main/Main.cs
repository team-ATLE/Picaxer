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
            Message.text = "Hello, your name is " + user.DisplayName + " and your email is " + user.Email;
        }
        else {
            Message.text = "No user.";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProfileClick() {
        SceneManager.LoadScene("Profile");
    }

    public void SignOutClick() {
        auth.SignOut();
        user.DeleteAsync();
        SceneManager.LoadScene("Sign");
    }
}
