using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

using static Post;
using static CommunityDAO;

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
    Dictionary<long, long> likes;
    int size = 20; // number of posts per page
    int currSize = 0; // first index of post in current page

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        if (user == null) {
            SceneManager.LoadScene("SignIn");
        }
        
        scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
        likes = new Dictionary<long, long>();

        // databasereference 전체 post 가져오는 부분
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        CommunityDAO dao = new CommunityDAO();
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(dao.getReferenceURL());
        GetRecent();
    }

    void GetRecent()
    {
        reference.Child("Post").OrderByChild("dateTime").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                data = task.Result;
                
                // Convert into List
                posts = new List<Post>();
                likes = new Dictionary<long, long>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Debug.Log(cur.Child("id").Value.ToString()); //
                    Post curPost = new Post(
                        long.Parse(cur.Child("id").Value.ToString()),
                        cur.Child("name").Value.ToString(),
                        cur.Child("email").Value.ToString(), 
                        cur.Child("imageURL").Value.ToString(), 
                        cur.Child("content").Value.ToString(), 
                        cur.Child("dateTime").Value.ToString(),
                        long.Parse(cur.Child("download_counts").Value.ToString())
                    );
                    posts.Add(curPost);
                    likes.Add(curPost.id, 0);
                    reference.Child("Like").OrderByChild("post").EqualTo(curPost.id).ValueChanged += LikeValueChanged;
                }
                posts.Reverse();
            }
        });
    }

    void GetPopular()
    {
        reference.Child("Post").OrderByChild("like_counts").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                data = task.Result;
                
                // Convert into List
                posts = new List<Post>();
                likes = new Dictionary<long, long>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Post curPost = new Post(
                        long.Parse(cur.Child("id").Value.ToString()),
                        cur.Child("name").Value.ToString(),
                        cur.Child("email").Value.ToString(), 
                        cur.Child("imageURL").Value.ToString(), 
                        cur.Child("content").Value.ToString(), 
                        cur.Child("dateTime").Value.ToString(),
                        long.Parse(cur.Child("download_counts").Value.ToString())
                    );
                    posts.Add(curPost);
                    likes.Add(curPost.id, 0);
                    reference.Child("Like").OrderByChild("post").EqualTo(curPost.id).ValueChanged += LikeValueChanged;
                }
                posts.Reverse();
            }
        });
    }

    void GetDownload()
    {
        reference.Child("Post").OrderByChild("download_counts").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                data = task.Result;
                
                // Convert into List
                posts = new List<Post>();
                likes = new Dictionary<long, long>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Post curPost = new Post(
                        long.Parse(cur.Child("id").Value.ToString()),
                        cur.Child("name").Value.ToString(),
                        cur.Child("email").Value.ToString(), 
                        cur.Child("imageURL").Value.ToString(), 
                        cur.Child("content").Value.ToString(), 
                        cur.Child("dateTime").Value.ToString(),
                        long.Parse(cur.Child("download_counts").Value.ToString())
                    );
                    posts.Add(curPost);
                    likes.Add(curPost.id, 0);
                    reference.Child("Like").OrderByChild("post").EqualTo(curPost.id).ValueChanged += LikeValueChanged;
                }
                posts.Reverse();
            }
        });
    }

    void LikeValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot data2 = args.Snapshot;
        if (data2 != null)
        {
            // find if the user likes ith post
            foreach (DataSnapshot cur in data2.Children)
            {
                likes[long.Parse(cur.Child("post").Value.ToString())] = data2.ChildrenCount;
            }
            Print();
        }
    }

    void Print() 
    {
        // Clear all existing buttons
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = currSize; i < size + currSize; i++)
        {
            if (i >= posts.Count) break;
            GameObject content = Instantiate(buttonPrefab, contentPanel);
            RawImage img = content.GetComponentInChildren<RawImage>();
            StartCoroutine(ImageLoad(img, posts[i].imageURL));
            TMP_Text[] text_content = content.GetComponentsInChildren<TMP_Text>();
            text_content[0].text = posts[i].name;
            text_content[1].text = posts[i].content;
            text_content[2].text = posts[i].dateTime;
            text_content[3].text = likes[posts[i].id].ToString();
            text_content[4].text = posts[i].download_counts.ToString();
            long post_id = posts[i].id;
            int index = i;
            Button[] buttons = content.GetComponentsInChildren<Button>();
            buttons[0].onClick.AddListener(() => UpdateLike(post_id));
            buttons[1].onClick.AddListener(() => DownloadFile(post_id, index));
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
        if (currSize - size < 0) return;
        currSize -= size;
        Print();
    }

    public void NextClick()
    {
        if (currSize + size >= posts.Count) return;
        currSize += size;
        Print();
    }

    void UpdateLike(long id) 
    {
        string key = "";
        Dictionary<string, object> values = new Dictionary<string, object>();
        values["user"] = user.Email;
        values["post"] = id;
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        

        reference.Child("Like").OrderByChild("post").EqualTo(id).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                foreach (var cur in task.Result.Children)
                {
                    key = cur.Key;
                    if (cur.Child("user").Value.ToString() == user.Email)
                    {
                        // delete
                        childUpdates["/Like/" + key] = null;
                        childUpdates["/Post/" + id + "/like_counts/"] = likes[id] - 1;
                        likes[id] -= 1;
                        reference.UpdateChildrenAsync(childUpdates);
                        Message.text = "Don't you like it? /_\\";
                        return;
                    }
                }

                // insert
                key = reference.Child("Like").Push().Key;
                childUpdates["/Like/" + key] = values;
                childUpdates["/Post/" + id+ "/like_counts/"] = likes[id] + 1;
                likes[id] += 1;
                reference.UpdateChildrenAsync(childUpdates);
                Message.text = "Do you like it? >_<";
            }
        });
    }

    void DownloadFile(long id, int index)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "DownloadedPng"); 
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Create local filesystem URL
        string imageURL = id.ToString() + ".png";
        string localFile = Path.Combine(Application.persistentDataPath, "DownloadedPng", imageURL);
        string localFile_uri = string.Format("{0}://{1}", Uri.UriSchemeFile, localFile);


        // Download to the local filesystem
        StorageReference imageRef = storageReference.Child("post").Child(imageURL);
        imageRef.GetFileAsync(localFile_uri).ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled) {
                Debug.Log("Successfully downloaded.");

                // update download counts
                Dictionary<string, object> childUpdates = new Dictionary<string, object>();
                posts[index].download_counts += 1;
                childUpdates["/Post/" + id + "/download_counts/"] = posts[index].download_counts + 1;
                reference.UpdateChildrenAsync(childUpdates);
                Debug.Log("Successfully update children");
                Message.text = "GOTCHA! Check your download tab.";
                Print();
            }
        });
    }

    public void BackClick()
    {
        SceneManager.LoadScene("Main");
    }

    public void NewPostClick()
    {
        SceneManager.LoadScene("CreatePost");
    }

    public void RecentClick()
    {
        GetRecent();
    }

    public void PopularClick()
    {
        GetPopular();
    }

    public void DownloadClick()
    {
        GetDownload();
    }
}
