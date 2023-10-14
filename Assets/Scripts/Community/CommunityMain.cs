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
    RawImage full_heart;
    RawImage empty_heart;

    DataSnapshot data;
    List<Post> posts;
    Dictionary<string, long> likes;
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
        likes = new Dictionary<string, long>();

        // databasereference 전체 post 가져오는 부분
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        GetRecent();
    }

    void GetRecent()
    {
        reference.Child("Post").OrderByChild("dateTime").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                data = task.Result;
                
                // Convert into List
                posts = new List<Post>();
                likes = new Dictionary<string, long>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Post curPost = new Post(
                        cur.Child("id").Value.ToString(),
                        cur.Child("name").Value.ToString(),
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

    void GetPopular()
    {
        reference.Child("Post").OrderByChild("like_counts").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                data = task.Result;
                
                // Convert into List
                posts = new List<Post>();
                likes = new Dictionary<string, long>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Post curPost = new Post(
                        cur.Child("id").Value.ToString(),
                        cur.Child("name").Value.ToString(),
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
        if (data2 != null)
        {
            // find if the user likes ith post
            foreach (DataSnapshot cur in data2.Children)
            {
                likes[cur.Child("post").Value.ToString()] = data2.ChildrenCount;
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
        currSize -= size;
        Print();
    }

    public void NextClick()
    {
        if (currSize + size >= posts.Count) return;
        currSize += size;
        Print();
    }

    void UpdateLike(string id) 
    {
        string key = "";
        Dictionary<string, object> values = new Dictionary<string, object>();
        values["user"] = user.Email;
        values["post"] = long.Parse(id);
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        

        reference.Child("Like").OrderByChild("post").EqualTo(long.Parse(id)).GetValueAsync().ContinueWithOnMainThread(task => {
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
                        return;
                    }
                }

                // insert
                key = reference.Child("Like").Push().Key;
                childUpdates["/Like/" + key] = values;
                childUpdates["/Post/" + id + "/like_counts/"] = likes[id] + 1;
                likes[id] += 1;
                reference.UpdateChildrenAsync(childUpdates);
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
}
