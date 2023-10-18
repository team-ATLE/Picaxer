using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

using TMPro;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using static CommunityDAO;
using static Post;

public class ProfileEditor : MonoBehaviour
{
    private string uid;
    public TMP_Text Email;
    public TMP_InputField Name;
    public System.Uri Photo_url;
    public TMP_Text Rank;
    public TMP_Text Rank_dec;
    public TMP_Text Score;
    public TMP_Text Total_likes;
    public TMP_Text Total_downloads;
    public TMP_Text Message;
    
    string imageName;
    
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    DatabaseReference reference;
    FirebaseStorage storage;
    StorageReference storageReference;
    RawImage photo;
    public GameObject buttonPrefab, post_list;
    public Transform contentPanel;
    ScrollRect scrollRect;
    DataSnapshot data;
    List<Post> posts;
    long total_likes = 0;
    long total_downloads = 0;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        if (user != null) {
            Email.text = user.Email;
            Name.text = user.DisplayName;
            Photo_url = user.PhotoUrl;
            uid = user.UserId;
        }
        else {
            Email.text = "";
            Name.text = "";
        }


        // Load profile photo
        CommunityDAO dao = new CommunityDAO();
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(dao.getReferenceURL());

        photo = gameObject.GetComponentInChildren<RawImage>();

        StartCoroutine(ProfileLoad(Convert.ToString(Photo_url)));

        // Load user's rank
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("Score").OrderByChild("highscore").ValueChanged += ScoreValueChanged;

        GetPost();
    }

    IEnumerator ProfileLoad(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            photo.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }

    void ScoreValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot data = args.Snapshot;
        long n = 0, total = data.ChildrenCount;
        if (data != null)
        {
            foreach (DataSnapshot cur in data.Children)
            {
                if (cur.Child("email").Value.ToString() == user.Email)
                {
                    string rankStr = "";
                    switch ((total - n) % 10)
                    {
                        case 1: rankStr = "st"; break;
                        case 2: rankStr = "nd"; break;
                        case 3: rankStr = "rd"; break;
                        default: rankStr = "th"; break;
                    }
                    Rank.text = (total - n).ToString();
                    Rank_dec.text = rankStr;
                    Score.text = "Score: " + cur.Child("highscore").Value.ToString();
                    break;
                }
                n += 1;
            }
        }
    }

    void GetPost()
    {
        reference.Child("Post").OrderByChild("email").EqualTo(user.Email).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                data = task.Result;
                
                // Convert into List
                posts = new List<Post>();
                foreach (DataSnapshot cur in data.Children)
                {
                    Post curPost = new Post(
                        long.Parse(cur.Child("id").Value.ToString()),
                        cur.Child("name").Value.ToString(),
                        cur.Child("email").Value.ToString(), 
                        cur.Child("imageURL").Value.ToString(), 
                        cur.Child("content").Value.ToString(), 
                        cur.Child("dateTime").Value.ToString(),
                        long.Parse(cur.Child("like_counts").Value.ToString()),
                        long.Parse(cur.Child("download_counts").Value.ToString())
                    );
                    total_likes += long.Parse(cur.Child("like_counts").Value.ToString());
                    total_downloads += long.Parse(cur.Child("download_counts").Value.ToString());
                    posts.Add(curPost);
                }
                posts.Reverse();
                if (data.ChildrenCount > 0) Print(data.ChildrenCount);
            }
        });
    }

    void Print(long count) 
    {
        // Clear all existing buttons
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        RectTransform rt = post_list.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(733, 300 * (count / 3 + (count % 3 > 0 ? 1 : 0))); // Mypostlist height 290 + space 10 = 300, 3 posts in each row
        
        foreach (Post post in posts)
        {
            GameObject content = Instantiate(buttonPrefab, contentPanel);
            RawImage img = content.GetComponentInChildren<RawImage>();
            StartCoroutine(ImageLoad(img, post.imageURL));
            TMP_Text[] text_content = content.GetComponentsInChildren<TMP_Text>();
            text_content[0].text = post.dateTime;
            text_content[1].text = post.like_counts.ToString();
            text_content[2].text = post.download_counts.ToString();
        }

        Total_likes.text = "Your arts got "+ total_likes +" like(s)!";
        Total_downloads.text = "Your arts have been downloaded "+ total_downloads +" time(s)!";
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

    public void SaveClick()
    {
        if (Name is not null)
        {
            Save(Name.text);
        }
        else
        {
            Message.text = "Name is empty.";
        }
        
    }

    void Save(string name)
    {
        if (user != null) {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile {
                DisplayName = name,
                PhotoUrl = Photo_url,
            };

            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("UpdateUserProfileAsync has error: " + task.Exception);
                    return;
                }

                Message.text = "User profile updated successfully.";
            });
            StartCoroutine(UploadWait());
        }
    }

    IEnumerator UploadWait() 
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Main");
    }
}
