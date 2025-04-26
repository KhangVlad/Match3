using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Firestore;


public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }


    public FirebaseUser User {get; private set; }
    public FirebaseApp App { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }   

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
        AuthenticationManager.Instance.OnAuthenticationSuccessfully += OnAuthenticationSuccessfully_SetUser;
    }

    private void OnDestroy()
    {
        AuthenticationManager.Instance.OnAuthenticationSuccessfully -= OnAuthenticationSuccessfully_SetUser;
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

                AuthenticationManager.Instance.SignInAnonymous(Auth, Firestore);
                //FetchServerTime();
                Debug.Log("Firebase initialized successfully!");
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
                Debug.Log("Update server time successfully");
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

}


[FirestoreData]
public struct ServerTimestamp
{
    [FirestoreProperty] public object UTCNow { get; set; }
}