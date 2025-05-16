#if !UNITY_WEBGL
using System;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using Match3.Shares;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }
    public event System.Action<FirebaseUser> OnAuthenticationSuccessfully;
    public event Action<bool> OnNewUserCreate;
    public bool IsNewUser = false;


    public bool IsUserDataLoaded;
    public bool IsSceneLoaded;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

        IsUserDataLoaded = false;
        IsSceneLoaded = false;
    }


    private void Start()
    {
    }


    public void HandleChangeScene()
    {
        if (IsUserDataLoaded)
        {
            if (IsSceneLoaded == false)
            {
                IsSceneLoaded = true;
                Loader.Load(Loader.Scene.Town);
            }
        }
    }


    // public void SignInAnonymous(FirebaseAuth auth, FirebaseFirestore firestore)
    // {
    //     auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
    //     {
    //         if (task.IsCanceled || task.IsFaulted)
    //         {
    //             Debug.LogError($"Anonymous sign-in failed: {task.Exception}");
    //             return;
    //         }
    //
    //         FirebaseUser user = task.Result.User;
    //         HandlerNewAndOldUserAnonymous(user, firestore);
    //         OnAuthenticationSuccessfully?.Invoke(user);
    //     });
    // }


    // private async void CreateNewUserDocument(string userID)
    // {
    //     Debug.Log($"CreateNewUserDocument  {userID}");
    //     DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users").Document(userID);
    //     UserData userData = UserManager.Instance.InitializeNewUserData();
    //
    //     //UserManager.Instance.InitializeUserEvents();
    //     //TimeManager.Instance.Initialize();
    //
    //     await docRef.SetAsync(userData).ContinueWithOnMainThread(task =>
    //     {
    //         if (task.IsCompleted)
    //         {
    //             IsUserDataLoaded = true;
    //             IsNewUser = true;
    //
    //             // HandleChangeScene();
    //         }
    //         else
    //         {
    //             Debug.LogError("Error creating new user document: " + task.Exception);
    //         }
    //     });
    //
    //     // DocumentReference shopDocRef = FirebaseManager.Instance.Firestore.Collection("shops").Document(userID);
    //     // ShopMarket market = ShopManager.Instance.InitializeMarket();
    //     // market.LoadMarket();
    //     // await shopDocRef.SetAsync(market).ContinueWithOnMainThread(task =>
    //     // {
    //     //     if (task.IsCompleted)
    //     //     {
    //     //         IsShopLoaded = true;
    //     //         HandleChangeScene();
    //     //     }
    //     //     else
    //     //     {
    //     //         Debug.LogError("Error creating new shop market document: " + task.Exception);
    //     //     }
    //     // });
    // }


  
    // private async void LoadUserData(DocumentSnapshot snapshot)
    // {
    //     if (snapshot == null)
    //     {
    //         Debug.LogError("Snapshot is null in LoadUserData");
    //         return;
    //     }
    //
    //     if (snapshot.Exists)
    //     {
    //         try
    //         {
    //             UserData localData = SaveManager.Instance.LoadUserDataFromLocalJson();
    //             if (localData == null)
    //             {
    //                 Debug.LogWarning("Local data is null, initializing new user data");
    //                 localData = UserManager.Instance.InitializeNewUserData();
    //             }
    //
    //             UserData cloud = snapshot.ConvertTo<UserData>();
    //             if (cloud == null)
    //             {
    //                 Debug.LogError("Failed to convert snapshot to UserData");
    //                 return;
    //             }
    //             if (snapshot.ContainsField("LastOnline"))
    //             {   
    //                 try
    //                 {
    //                     Timestamp lastOnlineTimestamp = snapshot.GetValue<Timestamp>("LastOnline");
    //                     if (lastOnlineTimestamp != null)
    //                     {
    //                         TimeManager.Instance.LastOnlineTime = lastOnlineTimestamp.ToDateTime();
    //                         var serverTime = await FirebaseManager.Instance.FetchServerTime();
    //                         if (serverTime != null)
    //                         {
    //                             DateTime loginTime = serverTime.ToDateTime();
    //
    //                             // Fix: Ensure cloud.LastOnline is properly converted to DateTime
    //                             DateTime lastOnlineTime = cloud.LastOnline is DateTime
    //                                 ? (DateTime)cloud.LastOnline
    //                                 : lastOnlineTimestamp.ToDateTime();
    //
    //                             TimeSpan timeDifference = loginTime - lastOnlineTime;
    //                             int minutesPassed = (int)timeDifference.TotalMinutes;
    //
    //                             cloud.Energy = localData.Energy + minutesPassed;
    //                             cloud.AvaiableBoosters = localData.AvaiableBoosters;
    //                             cloud.AllCharacterData = localData.AllCharacterData;
    //                             TimeManager.Instance.CheckNewDay(lastOnlineTimestamp);
    //                         }
    //                        
    //                     }
    //                 }
    //                 catch (Exception e)
    //                 {
    //                     Debug.LogError($"Error processing LastOnline: {e.Message}");
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.Log("Field 'LastOnlineSaved' does not exist in this document.");
    //             }
    //
    //             UserManager.Instance.UserData = cloud;
    //
    //             IsUserDataLoaded = true;
    //             HandleChangeScene();
    //         }
    //         catch (Exception ex)
    //         {
    //             Debug.LogError($"Error in LoadUserData: {ex.Message}");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("User document doesn't exist in Firestore");
    //     }
    // }

    // private async void HandlerNewAndOldUserAnonymous(FirebaseUser user, FirebaseFirestore firestore)
    // {
    //     Debug.Log("HandlerNewAndOldUserAnonymous");
    //     string userID = user.UserId;
    //     // Check if a document exists for this user in Firestore
    //     DocumentReference docRef = firestore.Collection("users").Document(userID);
    //     try
    //     {
    //         var serverTime = await FirebaseManager.Instance.FetchServerTime();
    //         TimeManager.Instance.LoginTime = serverTime.ToDateTime();
    //
    //         // Check if the document already exist
    //         DocumentSnapshot snapShot = await docRef.GetSnapshotAsync();
    //         if (snapShot.Exists)
    //         {
    //             LoadUserData(snapShot);
    //             OnNewUserCreate?.Invoke(false);
    //         }
    //         else
    //         {
    //             CreateNewUserDocument(userID);
    //             OnNewUserCreate?.Invoke(true);
    //             TimeManager.Instance.LastOnlineTime = TimeManager.Instance.LoginTime;
    //         }
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"Error: Error checking or creating document: {e.Message}");
    //     }
    // }

    public void SignInWithOutInternet()
    {
        LocalUserData data = SaveManager.Instance.LoadUserDataFromLocalJson();
        if (data == null)
        {
            data = UserManager.Instance.InitializeNewUserData();
            UserManager.Instance.UserData = data;
            IsNewUser = true;
        }
        else
        {
            Debug.Log("Found local user data. Loading offline mode.");
            IsNewUser = false;
        }

        UserManager.Instance.UserData = data;
        
        if (UserManager.Instance.UserData == null)
        {
            Debug.LogError("a");
        }
        IsUserDataLoaded = true;
        OnNewUserCreate?.Invoke(IsNewUser);
        // HandleChangeScene();
    }
}
#endif