using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Extensions;

public class CommunityMain : MonoBehaviour
{
    FirebaseAuth auth;
    DatabaseReference reference;
    FirebaseStorage storage;
    StorageReference storageReference;

    ScrollRect scrollRect;
    public TMP_Text Message;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null) {
            Message.text = "Hello, your name is " + user.DisplayName + " and your email is " + user.Email;
        }
        else {
            Message.text = "No user.";
        }
        getImages();
    }

    void getImages()
    {
        scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
        storage = FirebaseStorage.DefaultInstance;
        storageReference = storage.GetReferenceFromUrl("gs://picaxer-22bea.appspot.com");


        //
        RawImage img = scrollRect.content.GetChild(0).GetChild(0).GetComponent<RawImage>();
        StorageReference imageRef = storageReference.Child("post").Child("test.png");
        // Fetch the download URL
        imageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled) {
                Debug.Log("Download URL: " + task.Result); 
                try
                {
                    StartCoroutine(ImageLoad(img, Convert.ToString(task.Result)));
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex);
                    throw;
                }
                
            }
        });
        //
    }

    IEnumerator ImageLoad(RawImage img, string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            img.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
