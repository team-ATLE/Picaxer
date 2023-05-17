using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;

public class CommunityMain : MonoBehaviour
{
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    DatabaseReference reference;
    FirebaseStorage storage;
    StorageReference storageReference;

    ScrollRect scrollRect;
    public TMP_Text Message;

    DataSnapshot posts;
    long size; // 전체 posts 사이즈
    long currSize = 0; // 현재 posts 페이지의 최대 사이즈

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        if (user != null) {
            Message.text = "Hello, your name is " + user.DisplayName + " and your email is " + user.Email;
        }
        else {
            Message.text = "You need to login first.";
        }
        
        scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();

        // databasereference 전체 post 가져오는 부분
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        

        reference.Child("Post").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                posts = task.Result;
                size = posts.ChildrenCount; // 추후 size는 고정으로 변경
                Debug.Log(posts);
                Debug.Log(size);
                
                PrintRawImage(); 
            }
        });
    }

    void PrintRawImage() 
    {
        int i = 0;
        foreach (DataSnapshot post in posts.Children)
        {
            if (i >= size) break;
            RawImage img = scrollRect.content.GetChild(i).GetChild(0).GetComponent<RawImage>();
            TMP_Text text = scrollRect.content.GetChild(i).GetChild(1).GetComponent<TMP_Text>();
            StartCoroutine(ImageLoad(img, post.Child("imageURL").Value.ToString()));
            text.text = post.Child("content").Value.ToString();
            i++;
        }
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

    public void PrevClick()
    {
        Prev();
    }

    public void NextClick()
    {
        Next();
    }

    void Prev()
    {
        PrintRawImage();
        currSize -= 15;
    }

    void Next()
    {
        PrintRawImage();
        currSize += 15;
    }
}
