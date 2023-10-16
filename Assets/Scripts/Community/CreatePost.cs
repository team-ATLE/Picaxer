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
        reference.Child("Post").OrderByChild("id").LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCompleted) {
                foreach (DataSnapshot cur in task.Result.Children)
                {
                    Debug.Log(cur.Key);
                    id = long.Parse(cur.Child("id").Value.ToString()) + 1;
                    Debug.Log(id);
                    // id = long.Parse(cur.Key) + 1;
                }

                // storage에 로컬 이미지 업로드
                string localFile = Path.Combine(Application.persistentDataPath, "ExportedPng", imageName + ".png");
                string localFile_uri = string.Format("{0}://{1}", Uri.UriSchemeFile, localFile);
                string imageURL = id + ".png";

                // Create a reference to the file you want to upload
                StorageReference imageRef = storageReference.Child("post").Child(imageURL);

                // Upload the file to the path
                imageRef.PutFileAsync(localFile_uri).ContinueWith((Task<StorageMetadata> task2) => {
                    if (task2.IsFaulted || task2.IsCanceled) {
                        Debug.Log(task2.Exception.ToString());
                    }
                    else {
                        Message.text = "Finish uploading..."; // 개발환경에선 막힘, 근데 실행에서는 잘됨
                        Debug.Log("Finish uploading...");

                        // Change imageURL to result download URL.
                        imageURL = imageRef.GetDownloadUrlAsync().Result.ToString(); // Download URL
                        Debug.Log(imageURL);

                        Message.text = "Image uploaded";
                        Debug.Log("Image uploaded");

                        // Add date
                        string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        Post post = new Post(id, user.DisplayName, user.Email, imageURL, content, dateTime);
                        string json = JsonUtility.ToJson(post);
                        reference.Child("Post").Child(id.ToString()).SetRawJsonValueAsync(json);

                        Message.text = "Upload complete";
                        Debug.Log("Upload complete");
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
