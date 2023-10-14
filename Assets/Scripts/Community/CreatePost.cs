using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using System.Threading.Tasks;
using Firebase.Extensions;
using static Post;
using static CommunityDAO;

public class CreatePost : MonoBehaviour
{
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    DatabaseReference reference;
    StorageReference storageReference;
    CommunityDAO dao;
    string imageURL;
    string imageName = "";
    public TMP_Text Content;
    public TMP_Text Message;
    
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        CommunityDAO dao = new CommunityDAO();
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(dao.getReferenceURL());
        Message.text = "You selected: " + imageName;
        Debug.Log(user.DisplayName);
    }
    
    void Create(string content)
    {
        if (imageName == "") {
            Message.text = "No image selected";
            return;
        }
        long id = 0;
        reference.Child("Post").LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCompleted) {
                foreach (DataSnapshot cur in task.Result.Children)
                {
                    id = long.Parse(cur.Key) + 1;
                }

                // storage에 로컬 이미지 업로드
                string localFile = Path.Combine(Application.dataPath, "ExportedPng/", imageName + ".png"), imageURL = id + ".png";

                // Create a reference to the file you want to upload
                StorageReference imageRef = storageReference.Child("post").Child(imageURL);

                // Upload the file to the path
                imageRef.PutFileAsync(localFile).ContinueWith((Task<StorageMetadata> task2) => {
                    if (task2.IsFaulted || task2.IsCanceled) {
                        Debug.Log(task2.Exception.ToString());
                    }
                    else {
                        Debug.Log("Finish uploading...");

                        // Change imageURL to result download URL.
                        imageURL = imageRef.GetDownloadUrlAsync().Result.ToString(); // Download URL

                        Debug.Log("DONE");

                        // Add date
                        string dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        Post post = new Post(id.ToString(), user.DisplayName, user.Email, imageURL, content, dateTime);
                        string json = JsonUtility.ToJson(post);
                        reference.Child("Post").Child(id.ToString()).SetRawJsonValueAsync(json);
                        Debug.Log("Complete");

                        Message.text = "Upload complete";
                    }
                });
                StartCoroutine(UploadWait());
            }
            else {
                foreach (var e in task.Exception.Flatten().InnerExceptions) {
                    Debug.LogWarning($"Received Exception: {e.Message}");
                }
            }
        });
    }

    IEnumerator UploadWait() 
    {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene("CommunityMain");
    }

    public void UpdateImgURL(string name)
    {
        imageName = name;
        Message.text = "You selected: " + imageName;
    }

    public void PostClick() 
    {
        Create(Content.text);
    }

    public void BackClick()
    {
        SceneManager.LoadScene("CommunityMain");
    }
}
