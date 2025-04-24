using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Firestore;
using Match3.Shares;
using Match3;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }
    public event System.Action<FirebaseUser> OnAuthenticationSuccessfully;


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


    public void SignInAnonymous(FirebaseAuth auth, FirebaseFirestore firestore)
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"Anonymous sign-in failed: {task.Exception}");
                return;
            }

            FirebaseUser user = task.Result.User;
            HandlerNewAndOldUserAnonymous(user, firestore);

            OnAuthenticationSuccessfully?.Invoke(user);
            Debug.Log($"Signed in anonymously. User ID: {user.UserId}");
        });
    }

    // create new user document in Firestore
    private async void CreateNewUserDocument(string userID)
    {
        Debug.Log($"CreateNewUserDocument  {userID}");
        DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users").Document(userID);
        UserData userData = UserManager.Instance.InitializeNewUserData();

        //UserManager.Instance.InitializeUserEvents();
        //TimeManager.Instance.Initialize();

        await docRef.SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("New user document created successfully!");
                IsUserDataLoaded = true;
                HandleChangeScene();
            }
            else
            {
                Debug.LogError("Error creating new user document: " + task.Exception);
            }
        });

        // DocumentReference shopDocRef = FirebaseManager.Instance.Firestore.Collection("shops").Document(userID);
        // ShopMarket market = ShopManager.Instance.InitializeMarket();
        // market.LoadMarket();
        // await shopDocRef.SetAsync(market).ContinueWithOnMainThread(task =>
        // {
        //     if (task.IsCompleted)
        //     {
        //         IsShopLoaded = true;
        //         HandleChangeScene();
        //     }
        //     else
        //     {
        //         Debug.LogError("Error creating new shop market document: " + task.Exception);
        //     }
        // });
    }

    private void LoadUserData(DocumentSnapshot snapshot)
    {
        Debug.Log("LoadUserData");
        if (snapshot.Exists)
        {
            UserManager.Instance.UserData = snapshot.ConvertTo<UserData>();
            if (snapshot.ContainsField("LastOnline"))
            {
                Timestamp lastOnlineTimestamp = snapshot.GetValue<Timestamp>("LastOnline");
                TimeManager.Instance.LastOnlineTime = lastOnlineTimestamp.ToDateTime();
                Debug.Log($"Last online: {TimeManager.Instance.LastOnlineTime}");
            }
            else
            {
                Debug.Log("Field 'LastOnlineSaved' does not exist in this document.");
            }


            // Match3.Shares.Utilities.WaitAfterEndOfFrame(() =>
            // {
            //     UserManager.Instance.LoadUserCardDataSO(UserManager.Instance.UserData);
            //     IsUserDataLoaded = true;
            //     HandleChangeScene();
            // });

        }
    }
    private async void HandlerNewAndOldUserAnonymous(FirebaseUser user, FirebaseFirestore firestore)
    {
        Debug.Log("HandlerNewAndOldUserAnonymous");
        string userID = user.UserId;
        // Check if a document exists for this user in Firestore
        DocumentReference docRef = firestore.Collection("users").Document(userID);
        try
        {
            var serverTime = await FirebaseManager.Instance.FetchServerTime();
            TimeManager.Instance.LoginTime = serverTime.ToDateTime();

            // Check if the document already exist
            DocumentSnapshot snapShot = await docRef.GetSnapshotAsync();
            if (snapShot.Exists)
            {
                Debug.Log("Load level data");
                LoadUserData(snapShot);
            }
            else
            {
                CreateNewUserDocument(userID);
                TimeManager.Instance.LastOnlineTime = TimeManager.Instance.LoginTime;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error: Error checking or creating document: {e.Message}");
        }
    }
}