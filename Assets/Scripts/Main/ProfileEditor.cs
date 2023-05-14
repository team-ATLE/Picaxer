using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
// using Firebase.Database;

public class ProfileEditor : MonoBehaviour
{
    public TMP_Text Email;
    public TMP_InputField Name;
    public System.Uri Photo_url;
    public TMP_Text Message;
    string res;
    FirebaseAuth auth;
    // DatabaseReference reference;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null) {
            Email.text = user.Email;
            Name.text = user.DisplayName;
            Photo_url = user.PhotoUrl;
            string uid = user.UserId;
        }
        else {
            Email.text = "email.com";
            Name.text = "asdf";
        }
    }

    void Update()
    {
        Message.text = res;
    }

    public void SaveClick()
    {
        if (Name is not null)
        {
            // storage에 이미지 저장
            Save(Name.text);
        }
        else
        {
            res = "Field Name is empty.";
        }
        
    }

    void Save(string name)
    {
        // button onclick
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null) {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile {
                DisplayName = name,
                PhotoUrl = Photo_url,
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("UpdateUserProfileAsync has error: " + task.Exception);
                    return;
                }

                Message.text = "User profile updated successfully.";
            });
        }
    }
}
