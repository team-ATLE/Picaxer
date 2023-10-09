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
}