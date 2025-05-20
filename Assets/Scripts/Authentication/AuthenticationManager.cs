// #if !UNITY_WEBGL
// using System;
// using System.Collections;
// using System.Threading.Tasks;
// using UnityEngine;
// using Firebase.Auth;
// using Firebase.Extensions;
// using GooglePlayGames;
// using GooglePlayGames.BasicApi;
// using Match3.Shares;
//
//
// public class AuthenticationManager : MonoBehaviour
// {
//     public static AuthenticationManager Instance { get; private set; }
//
//     public event Action OnUserDataLoaded;
//
//     public bool IsSceneLoaded { get; private set; }
//
//     private FirebaseAuth auth;
//
//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//
//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//     }
//
//     private void Start()
//     {
//         FirebaseManager.Instance.OnFirebaseInitialized += InitializeFirebase;
//     }
//
//     private void InitializeFirebase()
//     {
//         auth = FirebaseManager.Instance.Auth;
//         LoadLocalUserData();
//     }
//
//     private void LoadLocalUserData()
//     {
//         var data = SaveManager.Instance.LoadUserDataFromLocalJson();
//         UserManager.Instance.UserData = data;
//         OnUserDataLoaded?.Invoke();
//     }
//
//     public void HandleChangeScene()
//     {
//         if (IsSceneLoaded) return;
//
//         IsSceneLoaded = true;
//         Loader.Load(Loader.Scene.Town);
//     }
//
//     public IEnumerator CheckOrCreateUser()
//     {
//         yield return AuthenticateWithPlayGames(async success =>
//         {
//             if (success)
//             {
//                 await SignInToFirebaseWithCHPlay();
//                 LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
//             }
//             else
//             {
//                 LinkGooglePlayAccount(() =>
//                 {
//                     string chPlayId = GetCHPlayUserID();
//                     UserManager.Instance.UserData = UserManager.Instance.InitializeNewUserData(chPlayId);
//                     LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
//                 });
//             }
//         });
//     }
//
//     public async Task SignInToFirebaseWithCHPlay()
//     {
//         string userId = GetCHPlayUserID();
//         var userData = await FirebaseManager.Instance.GetUserDataFromFirebase(userId);
//
//         if (userData == null)
//         {
//             userData = UserManager.Instance.InitializeNewUserData(userId);
//         }
//         else
//         {
//             UserManager.Instance.ModifyUserID(userId);
//         }
//
//         UserManager.Instance.UserData = userData;
//     }
//
//     #region Google Play Games Integration
//
//     public void LinkGooglePlayAccount(Action onSuccess = null, Action onFailure = null)
//     {
//         StartCoroutine(AuthenticateWithPlayGames(success =>
//         {
//             if (!success)
//             {
//                 Debug.LogWarning("Failed to authenticate with GPGS.");
//                 onFailure?.Invoke();
//                 return;
//             }
//
//             PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
//             {
//                 var credential = PlayGamesAuthProvider.GetCredential(code);
//                 LinkFirebaseAccount(credential, onSuccess, onFailure);
//             });
//         }));
//     }
//
//
//     public IEnumerator AuthenticateWithPlayGames(Action<bool> callback)
//     {
//         float timeout = 5f;
//         float elapsed = 0f;
//
//         while (PlayGamesPlatform.Instance == null && elapsed < timeout)
//         {
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
//
//         if (PlayGamesPlatform.Instance == null)
//         {
//             Debug.LogError("GPGS platform not initialized.");
//             callback(false);
//             yield break;
//         }
//
//         PlayGamesPlatform.Instance.Authenticate(status =>
//         {
//             bool success = status == SignInStatus.Success;
//
//             if (success)
//             {
//                 PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
//                 {
//                     var credential = PlayGamesAuthProvider.GetCredential(code);
//                     auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
//                     {
//                         callback(task.IsCompleted && !task.IsFaulted);
//                     });
//                 });
//             }
//             else
//             {
//                 callback(false);
//             }
//         });
//     }
//
//     public void LinkFirebaseAccount(Credential credential, Action onSuccess, Action onFailure = null)
//     {
//         if (auth.CurrentUser == null)
//         {
//             Debug.LogError("No Firebase user is currently signed in.");
//             onFailure?.Invoke();
//             return;
//         }
//
//         auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
//         {
//             if (task.IsCompleted && !task.IsFaulted)
//             {
//                 Debug.Log("Successfully linked Firebase with GPGS.");
//                 onSuccess?.Invoke();
//             }
//             else
//             {
//                 Debug.LogError("Firebase account link failed: " + task.Exception);
//                 onFailure?.Invoke();
//             }
//         });
//     }
//
//     public string GetCHPlayUserID()
//     {
//         return PlayGamesPlatform.Instance?.GetUserId();
//     }
//
//     #endregion
// }
// #endif

#if !UNITY_WEBGL
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Match3.Shares;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    public event Action OnUserDataLoaded;

    public bool IsSceneLoaded { get; private set; }

    private FirebaseAuth auth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        FirebaseManager.Instance.OnFirebaseInitialized += InitializeFirebase;
    }

    private void InitializeFirebase()
    {
        auth = FirebaseManager.Instance.Auth;
        LoadUserDataFromLocal();
    }

    private void LoadUserDataFromLocal()
    {
        var data = SaveManager.Instance.LoadUserDataFromLocalJson();
        UserManager.Instance.UserData = data;
        OnUserDataLoaded?.Invoke();
    }

    public void HandleSceneLoad()
    {
        if (IsSceneLoaded) return;

        IsSceneLoaded = true;
        Loader.Load(Loader.Scene.Town);
    }

    public IEnumerator CheckOrCreateUser()
    {
        yield return AuthenticateWithPlayGames(async success =>
        {
            if (success)
            {
                await SignInToFirebaseWithPlayGames();
                LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
            }
            else
            {
                LinkGooglePlayAccount(
                    onSuccess: () =>
                    {
                        var newUserId = GetPlayGamesUserID();
                        var newUserData = UserManager.Instance.InitializeNewUserData(newUserId);
                        UserManager.Instance.UserData = newUserData;
                        LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
                    },
                    onFailure: () =>
                    {
                        Debug.LogError("Failed to link Google Play account and create user.");
                    });
            }
        });
    }
    
    
    public bool IsCHPlayLinkedToFirebase()
    {
        if (auth?.CurrentUser == null)
        {
            Debug.LogWarning("No Firebase user is signed in.");
            return false;
        }

        // Check if the current user was signed in using Google Play Games
        // The ProviderId for Google Play Games is "playgames.google.com"
        foreach (var info in auth.CurrentUser.ProviderData)
        {
            if (info.ProviderId == "playgames.google.com")
            {
                Debug.Log("Google Play Games is linked to Firebase.");
                return true;    
            }
        }

        Debug.Log("Google Play Games is not linked to Firebase.");
        return false;
    }

    public async Task SignInToFirebaseWithPlayGames()
    {
        string userId = GetPlayGamesUserID();

        var userData = await FirebaseManager.Instance.GetUserDataFromFirebase(userId);

        if (userData == null)
        {
            userData = UserManager.Instance.InitializeNewUserData(userId);
        }
        else
        {
            UserManager.Instance.ModifyUserID(userId);
        }

        UserManager.Instance.UserData = userData;
    }

    #region Google Play Games Integration

    public void LinkGooglePlayAccount(Action onSuccess = null, Action onFailure = null)
    {
        StartCoroutine(AuthenticateWithPlayGames(success =>
        {
            if (!success)
            {
                Debug.LogWarning("GPGS authentication failed.");
                onFailure?.Invoke();
                return;
            }

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
            {
                var credential = PlayGamesAuthProvider.GetCredential(code);
                LinkFirebaseAccount(credential, onSuccess, onFailure);
            });
        }));
    }

    public IEnumerator AuthenticateWithPlayGames(Action<bool> callback)
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (PlayGamesPlatform.Instance == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (PlayGamesPlatform.Instance == null)
        {
            Debug.LogError("Google Play Games Platform not initialized.");
            callback(false);
            yield break;
        }

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            bool success = status == SignInStatus.Success;

            if (!success)
            {
                callback(false);
                return;
            }

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
            {
                var credential = PlayGamesAuthProvider.GetCredential(code);

                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                        callback(true);
                    else
                    {
                        Debug.LogError("Firebase sign-in failed: " + task.Exception);
                        callback(false);
                    }
                });
            });
        });
    }

    public void LinkFirebaseAccount(Credential credential, Action onSuccess, Action onFailure = null)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No Firebase user currently signed in.");
            onFailure?.Invoke();
            return;
        }

        auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Successfully linked Firebase with Google Play.");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Linking Firebase account failed: " + task.Exception);
                onFailure?.Invoke();
            }
        });
    }

    public string GetPlayGamesUserID()
    {
        return PlayGamesPlatform.Instance?.GetUserId();
    }

    #endregion
}
#endif
