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
    Dictionary<string, long> likes;
    int size = 3; // 페이지별 포스트 개수
    int currSize = 0; // 현재 페이지의 첫 번째 포스트

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
        likes = new Dictionary<string, long>();

        // databasereference 전체 post 가져오는 부분
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("Post").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                data = task.Result;
                
                // Convert into List
                posts = new List<Post>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Post curPost = new Post(
                        cur.Child("id").Value.ToString(),
                        cur.Child("email").Value.ToString(), 
                        cur.Child("imageURL").Value.ToString(), 
                        cur.Child("content").Value.ToString(), 
                        cur.Child("dateTime").Value.ToString()
                    );
                    posts.Add(curPost);
                    likes.Add(curPost.id, 0);
                    reference.Child("Like").OrderByChild("post").EqualTo(long.Parse(curPost.id)).ValueChanged += LikeValueChanged;
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
        Debug.Log("LikeValueChanged");
        if (data2 != null)
        {
            // find if the user likes ith post
            foreach (DataSnapshot cur in data2.Children)
            {
                likes[cur.Child("post").Value.ToString()] = data2.ChildrenCount;
                if (user.Email == cur.Child("user").Value.ToString())
                {
                    Debug.Log("True");
                    // button.GetComponentInChildren<Image>();
                    break;
                }
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
            content.GetComponentInChildren<TMP_Text>().text = posts[i].id + "\n" + posts[i].content + "\n" + posts[i].email + "\n" + posts[i].dateTime + "\n" + likes[posts[i].id];
            string post_id = posts[i].id;
            content.GetComponentInChildren<Button>().onClick.AddListener(() => UpdateLike(post_id));
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
        Debug.Log(currSize);
        currSize -= size;
        Print();
    }

    public void NextClick()
    {
        if (currSize + size >= posts.Count) return;
        Debug.Log(currSize);
        currSize += size;
        Print();
    }

    void UpdateLike(string id) 
    {
        Debug.Log(id + " like click");
        string key = "";
        Dictionary<string, object> values = new Dictionary<string, object>();
        values["user"] = user.Email;
        values["post"] = long.Parse(id);
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        

        reference.Child("Like").OrderByChild("post").EqualTo(long.Parse(id)).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                Debug.Log(task.Result);
                Debug.Log(task.Result.ChildrenCount);
                if (task.Result.ChildrenCount != 0) {
                    // delete
                    Debug.Log("Executed1");
                    foreach (var cur in task.Result.Children)
                    {
                        key = cur.Key;
                        Debug.Log(cur.Child("user").Value.ToString());
                        likes[cur.Child("post").Value.ToString()] -= 1;
                    }
                    childUpdates["/Like/" + key] = null;
                    Debug.Log("Executed2");
                }
                else {
                    Debug.Log("Executed3");
                    // insert
                    key = reference.Child("Like").Push().Key;
                    childUpdates["/Like/" + key] = values;
                    
                    Debug.Log("Executed4");
                }
                reference.UpdateChildrenAsync(childUpdates);
                Debug.Log("Executed5");
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
}
