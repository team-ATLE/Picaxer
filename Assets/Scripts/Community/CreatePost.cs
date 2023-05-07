using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using static Post;

public class CreatePost : MonoBehaviour
{
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    DatabaseReference reference;
    FirebaseStorage storage;
    public TMP_Text Content;
    
    // Start is called before the first frame update
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        storage = FirebaseStorage.DefaultInstance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void Create(string imageURL, string content)
    {
        Post post = new Post(user.Email, imageURL, content);
        string json = JsonUtility.ToJson(post);

        reference.Child("post").Child("1").SetRawJsonValueAsync(json);
    }

    public void PostClick() 
    {
        Create("1", Content.text);
    }
}
