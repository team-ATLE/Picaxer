using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Storage;

public class LoadProfilePhoto : MonoBehaviour
{
    RawImage rawImage;
    FirebaseAuth auth;
    FirebaseStorage storage;
    StorageReference storageReference;

    IEnumerator imageLoad(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            rawImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    void Start()
    {
        rawImage = gameObject.GetComponent<RawImage>();
        
        auth = FirebaseAuth.DefaultInstance;
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;

        storage = FirebaseStorage.DefaultInstance;
        storageReference = storage.GetReferenceFromUrl("gs://picaxer-22bea.appspot.com");

        //get reference of image
        StorageReference image = storageReference.Child("profile").Child(user.UserId+".jpg");

        //get the download link of file
        image.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                StartCoroutine(imageLoad(Convert.ToString(task.Result)));
            }
        });
    }

    void Update()
    {
        
    }
}
