using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    string imageURL;
    public TMP_Text Content;
    
    // Start is called before the first frame update
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        CommunityDAO dao = new CommunityDAO();
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(dao.getReferenceURL());
        Debug.Log(dao.getReferenceURL());
    }
    
    void Create(string content)
    {
        long id;
        reference.Child("Post").GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCompleted) {
                id = task.Result.ChildrenCount + 1;

                // storage에 로컬 이미지 업로드
                // File located on disk
                string localFile = "./Images/test.png", imageURL = id + ".png";

                // Create a reference to the file you want to upload
                StorageReference imageRef = storageReference.Child("post").Child(imageURL);

                // Upload the file to the path
                imageRef.PutFileAsync(localFile).ContinueWith((Task<StorageMetadata> task2) => {
                    if (task2.IsFaulted || task2.IsCanceled) {
                        // Uh-oh, an error occurred!
                        Debug.Log(task2.Exception.ToString());
                    }
                    else {
                        // Metadata contains file metadata such as size, content-type, and download URL.
                        // StorageMetadata metadata = task2.Result;
                        // string md5Hash = metadata.Md5Hash;
                        Debug.Log("Finished uploading...");
                        // Debug.Log("md5 hash = " + md5Hash);

                        // Change imageURL to result download URL.
                        imageURL = imageRef.GetDownloadUrlAsync().Result.ToString();
                        Debug.Log(imageURL); // Download URL

                        Post post = new Post(user.Email, imageURL, content);
                        string json = JsonUtility.ToJson(post);
                        reference.Child("Post").Child(id.ToString()).SetRawJsonValueAsync(json);

                        Debug.Log("Upload complete");
                    }
                });
            }
        });
    }

    public void PostClick() 
    {
        Create(Content.text);
    }
}
