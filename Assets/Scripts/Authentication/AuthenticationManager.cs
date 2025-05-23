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
using Google;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    public event Action OnUserDataLoaded;

    public bool IsSceneLoaded { get; private set; }

    private FirebaseAuth auth;
    private bool isAuthenticating = false;

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
        InitializeGoogleSignIn();
        FirebaseManager.Instance.OnFirebaseInitialized += InitializeFirebase;
    }

    private void InitializeFirebase()
    {
        auth = FirebaseManager.Instance.Auth;
        LoadLocalUserData();
    }

    private void LoadLocalUserData()
    {
        var data = SaveManager.Instance.LoadUserDataFromLocalJson();
        UserManager.Instance.UserData = data;
        OnUserDataLoaded?.Invoke();
    }

    public void HandleChangeScene()
    {
        if (IsSceneLoaded) return;

        IsSceneLoaded = true;
        Loader.Load(Loader.Scene.Town);
    }

    

    public IEnumerator AuthenticateWithPlayGames(Action<bool> callback)
    {
        const float timeout = 15f; // Increase timeout for slower connections
        float elapsed = 0f;

        // Add debug logs
        Debug.Log("Starting Play Games authentication...");

        while (PlayGamesPlatform.Instance == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (PlayGamesPlatform.Instance == null)
        {
            Debug.LogError("Google Play Games Platform not initialized after timeout.");
            callback(false);
            yield break;
        }

        bool authenticationComplete = false;
        Exception authException = null;

        try
        {
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                try
                {
                    Debug.Log($"Play Games Authentication Status: {status}");
                    bool success = status == SignInStatus.Success;

                    if (!success)
                    {
                        Debug.LogWarning($"Play Games authentication failed with status: {status}");
                        authenticationComplete = true;
                        callback(false);
                        return;
                    }

                    Debug.Log("Requesting server side access...");
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(code))
                            {
                                Debug.LogError("Server side access code is null or empty");
                                authenticationComplete = true;
                                callback(false);
                                return;
                            }

                            Debug.Log("Got server side access code, creating credential...");
                            var credential = PlayGamesAuthProvider.GetCredential(code);

                            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                            {
                                try
                                {
                                    if (task.IsCanceled)
                                    {
                                        Debug.LogError("Firebase sign-in was canceled");
                                        authenticationComplete = true;
                                        callback(false);
                                        return;
                                    }

                                    if (task.IsFaulted)
                                    {
                                        Debug.LogError($"Firebase sign-in failed: {task.Exception}");
                                        authenticationComplete = true;
                                        callback(false);
                                        return;
                                    }

                                    Debug.Log("Firebase sign-in successful");
                                    authenticationComplete = true;
                                    callback(true);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Exception in Firebase sign-in completion: {ex.Message}\n{ex.StackTrace}");
                                    authenticationComplete = true;
                                    callback(false);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Exception in RequestServerSideAccess: {ex.Message}\n{ex.StackTrace}");
                            authenticationComplete = true;
                            callback(false);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception in Authenticate callback: {ex.Message}\n{ex.StackTrace}");
                    authException = ex;
                    authenticationComplete = true;
                    callback(false);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception initiating authentication: {ex.Message}\n{ex.StackTrace}");
            authException = ex;
            authenticationComplete = true;
            callback(false);
        }

        // Wait for authentication to complete or timeout
        elapsed = 0f;
        while (!authenticationComplete && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!authenticationComplete)
        {
            Debug.LogError($"Play Games authentication timed out after {timeout} seconds");
            callback(false);
        }
        else if (authException != null)
        {
            Debug.LogError($"Authentication failed with exception: {authException.Message}");
        }
    }

    public void LinkFirebaseAccount(Credential credential, string linkedService, Action onSuccess, Action onFailure = null)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No Firebase user currently signed in.");
            onFailure?.Invoke();
            return;
        }

        auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"Successfully linked Firebase with {linkedService}.");
                
                // Update LinkedCredential in UserData
                if (UserManager.Instance != null && UserManager.Instance.UserData != null)
                {
                    UserManager.Instance.UserData.LinkedCredential = linkedService;
                }
                
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



    #region Google Sign In

    private const string webClientId = "938858293143-qpf14n5mt1ag2jguvt9amit9muji4cq1.apps.googleusercontent.com"; 
    private GoogleSignInConfiguration configuration;
    
    private void InitializeGoogleSignIn()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            UseGameSignIn = false,
            RequestEmail = true
        };
    }

    public void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
            OnAuthenticationFinished, TaskScheduler.FromCurrentSynchronizationContext());
    }
    
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Google Sign-In failed: " + task.Exception);
            isAuthenticating = false;
        }
        else if (task.IsCanceled)
        {
            Debug.LogError("Google Sign-In was cancelled");
            isAuthenticating = false;
        }
        else
        {
            var googleUser = task.Result;
            Debug.Log("Google Sign-In successful: " + googleUser.Email);
            // Use the ID token to sign in to Firebase
            if (!string.IsNullOrEmpty(googleUser.IdToken))
            {
                Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(async authTask => {
                    if (authTask.IsCompleted && !authTask.IsFaulted)
                    {
                        Debug.Log("Firebase sign-in with Google successful");
                        string userId = auth.CurrentUser.UserId;
                        var userData = await FirebaseManager.Instance.GetUserDataFromFirebase(userId);
                        
                        if (userData == null)
                        {
                            // Create new user data if none exists
                            userData = UserManager.Instance.InitializeNewUserData(userId);
                            userData.LinkedCredential = "Google";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(userData.LinkedCredential))
                            {
                                userData.LinkedCredential = "Google";
                            }
                        }
                        
                        UserManager.Instance.UserData = userData;
                        LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
                    }
                    else
                    {
                        Debug.LogError("Firebase sign-in with Google failed: " + authTask.Exception);
                    }
                    
                    isAuthenticating = false;
                });
            }
            else
            {
                isAuthenticating = false;
            }
        }
    }

    #endregion

    public IEnumerator HandleGuestSignIn(Action onComplete = null)
    {
        if (isAuthenticating) yield break;
        isAuthenticating = true;

        // Generate a unique guest ID
        string guestId = UserManager.Instance.GenerateUniqueUserID();

        try
        {
            // Initialize new user data locally without Firebase anonymous auth
            var userData = UserManager.Instance.InitializeNewUserData(guestId);
            userData.LinkedCredential = "Guest"; // Mark as guest account
            UserManager.Instance.UserData = userData;
            
            // Save the guest data locally
            SaveManager.Instance.SaveUserDataToLocalJson();
            
            Debug.Log($"Guest user created with ID: {guestId}");
            onComplete?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Guest sign-in error: {ex.Message}");
        }

        isAuthenticating = false;
    }

    public IEnumerator HandleChPlaySignIn(Action onComplete = null)
    {
        if (isAuthenticating) yield break;
        isAuthenticating = true;

        bool success = false;

        // Step 1: Authenticate with Play Games
        yield return AuthenticateWithPlayGames(result => { success = result; });

        if (!success)
        {
            Debug.LogError("Failed to authenticate with Play Games");
            isAuthenticating = false;
            onComplete?.Invoke();
            yield break;
        }

        // Step 2: Proceed only if authentication was successful
        string userId = GetPlayGamesUserID();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("CH Play authentication failed: User ID is null or empty");
            isAuthenticating = false;
            onComplete?.Invoke();
            yield break;
        }

        Debug.Log($"CH Play authentication successful. User ID: {userId}");

        // Step 3: Fetch or create user data
        var userDataTask = FirebaseManager.Instance.GetUserDataFromFirebase(userId);
        yield return new WaitUntil(() => userDataTask.IsCompleted);

        LocalUserData userData = null;

        try
        {
            userData = userDataTask.Result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while getting Firebase user data: {ex.Message}\n{ex.StackTrace}");
        }

        if (userData == null)
        {
            userData = UserManager.Instance.InitializeNewUserData(userId);
            userData.LinkedCredential = "GooglePlay";
            Debug.Log("Created new user data for CH Play user");
        }
        else
        {
            Debug.Log("Found existing user data for CH Play user");
            UserManager.Instance.ModifyUserID(userId);
            if (string.IsNullOrEmpty(userData.LinkedCredential))
            {
                userData.LinkedCredential = "GooglePlay";
            }
        }

        UserManager.Instance.UserData = userData;
        onComplete?.Invoke();
        isAuthenticating = false;
    }

    public void HandleGoogleSignIn()
    {
        if (isAuthenticating) return;
        isAuthenticating = true;
        OnSignIn();
    }
}
#endif
