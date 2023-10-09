using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

using TMPro;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Storage;
using static CommunityDAO;

public class ProfileEditor : MonoBehaviour
{
    private string uid;
    public TMP_Text Email;
    public TMP_InputField Name;
    public System.Uri Photo_url;
    public TMP_Text Message;
    
    string imageName;
    
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    FirebaseStorage storage;
    StorageReference storageReference;
    RawImage photo;
    // DatabaseReference reference;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        if (user != null) {
            Email.text = user.Email;
            Name.text = user.DisplayName;
            Photo_url = user.PhotoUrl;
            uid = user.UserId;
        }
        else {
            Email.text = "";
            Name.text = "";
        }

        CommunityDAO dao = new CommunityDAO();
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(dao.getReferenceURL());

        photo = gameObject.GetComponentInChildren<RawImage>();

        StartCoroutine(ProfileLoad(Convert.ToString(Photo_url)));
    }

    IEnumerator ProfileLoad(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            photo.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    public void SaveClick()
    {
        if (Name is not null)
        {
            Save(Name.text);
        }
        else
        {
            Message.text = "Name is empty.";
        }
        
    }

    void Save(string name)
    {
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
            StartCoroutine(UploadWait());
        }
    }

    IEnumerator UploadWait() 
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Main");
    }
}
