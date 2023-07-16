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

    public GameObject buttonPrefab;
    public Transform contentPanel;
    ScrollRect scrollRect;
    public TMP_Text Message;

    DataSnapshot data;
    List<Post> posts;
    long size = 0; // 전체 posts 사이즈
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
                data = task.Result;
                size = data.ChildrenCount;
                
                // Convert into List
                posts = new List<Post>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Post curPost = new Post(
                        cur.Child("email").Value.ToString(), 
                        cur.Child("imageURL").Value.ToString(), 
                        cur.Child("content").Value.ToString(), 
                        cur.Child("dateTime").Value.ToString()
                    );
                    posts.Add(curPost);
                }
                posts.Reverse();
                PrintRawImage(); 
            }
        });
    }

    void PrintRawImage() 
    {
        // Clear all existing buttons
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        
        int i = 0;
        foreach (Post post in posts)
        {
            if (i >= size) break;
            GameObject button = Instantiate(buttonPrefab, contentPanel);
            RawImage img = button.GetComponentInChildren<RawImage>();
            StartCoroutine(ImageLoad(img, post.imageURL));
            button.GetComponentInChildren<TMP_Text>().text = post.content + "\n" + post.dateTime;
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

    public void BackClick()
    {
        SceneManager.LoadScene("Main");
    }

    public void NewPostClick()
    {
        SceneManager.LoadScene("CreatePost");
    }
}
