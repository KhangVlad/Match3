#if !UNITY_WEBGL
using System;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Firestore;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Match3.Shares;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseUser User { get; private set; }
    public FirebaseApp App { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }

    public event Action OnFirebaseInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        InitializeFirebase();
    }


    public void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Firestore = FirebaseFirestore.DefaultInstance;
                OnFirebaseInitialized?.Invoke();
   
            }
            else
            {
                Debug.LogError($"Firebase initialization failed: {task.Result}");
            }
        });
    }
    
   
    
    

    public async Task<Timestamp> FetchServerTime()
    {
        Debug.LogWarning("FetchServerTime ~~~~~~~~~~~~ Performance issue !!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Timestamp now = default;
        DocumentReference docRef = Firestore.Collection("server_time").Document("Time");
        ServerTimestamp serverTimestamp = new ServerTimestamp()
        {
            UTCNow = FieldValue.ServerTimestamp
        };
        await docRef.SetAsync(serverTimestamp).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
            }
            else
            {
                Debug.LogError("Error creating new user document: " + task.Exception);
            }
        });

        await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    if (snapshot.ContainsField("UTCNow"))
                    {
                        now = snapshot.GetValue<Timestamp>("UTCNow");
                    }
                }
                else
                {
                    Debug.LogError("not have this snapshot");
                }
            }
            else
            {
                Debug.LogError($"Error checking user data: {task.Exception}");
            }
        });
        return now;
    }

    private void OnAuthenticationSuccessfully_SetUser(FirebaseUser user)
    {
        this.User = user;
    }

    public async Task<LocalUserData> GetUserDataFromFirebase(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Attempted to fetch user data with empty ID");
                return null;
            }

            DocumentReference docRef = Firestore.Collection("users").Document(id);
            DocumentSnapshot snapShot = await docRef.GetSnapshotAsync();

            if (snapShot.Exists)
            {
                Debug.Log($"Found existing user data for ID: {id}");
                UserData userData = snapShot.ConvertTo<UserData>();
                return new LocalUserData(userData);
            }

            Debug.Log($"No existing user data found for ID: {id}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error retrieving user data: {ex.Message}");
            return null;
        }
    }
}


[FirestoreData]
public struct ServerTimestamp
{
    [FirestoreProperty] public object UTCNow { get; set; }
}
#endif
