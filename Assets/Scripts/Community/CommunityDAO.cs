using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Extensions;

using static User;
using static Post;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CommunityDAO {
    FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    DatabaseReference reference;
    StorageReference storageReference;
    string refURL = "gs://picaxer-22bea.appspot.com";

    public CommunityDAO() {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(refURL);
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Auth
    public bool isSignin() {
        if (getUserId() != null) return true;
        else return false;
    }

    public string getUserId() {
        return user.UserId;
    }

    public string getUserEmail() {
        return user.Email;
    }

    public string getUserName() {
        return user.DisplayName;
    }



    // Storage
    public string getReferenceURL() {
        return refURL;
    }

    /* 

    public string getImage(string imageName) {
        StorageReference imageRef = storageReference.Child("post").Child(imageName);
        // Fetch the download URL
        imageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled) {
                // Debug.log("Download URL: " + task.Result); 
                return task.Result.ToString();
            }
            else return null;
        });
        return null;
    }


    // Database
    public DataSnapshot getAllPosts(string table) {
        reference.Child(table).GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCompleted) {
                return task.Result;
            }
            else return null;
        });
        return;
    }

    public long getAllPostsCount(DataSnapshot data) {
        if (data != null) return data.ChildrenCount;
        else return 0;
    }

    */
}