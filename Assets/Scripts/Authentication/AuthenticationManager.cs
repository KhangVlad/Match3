#if !UNITY_WEBGL
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Auth;
using Match3.Shares;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    // public event Action NewUser;
    // public event Action OldUser;
    public event Action OnUserDataLoaded;
    public bool IsSceneLoaded;
    private FirebaseAuth auth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        IsSceneLoaded = false;
    }

    private void Start()
    {
        FirebaseManager.Instance.OnFirebaseInitialized += () =>
        {
            auth = FirebaseManager.Instance.Auth;
            UserDataLoad();

            // StartCoroutine(AutoSignInCoroutine());
        };
    }

    private void UserDataLoad()
    {
        LocalUserData data = SaveManager.Instance.LoadUserDataFromLocalJson();
        UserManager.Instance.UserData = data;
        OnUserDataLoaded?.Invoke();
    }

    private IEnumerator AutoSignInCoroutine()
    {
        LocalUserData data = SaveManager.Instance.LoadUserDataFromLocalJson();

        if (data == null)
        {
            // Wait for GPGS to be ready, then try sign-in
            yield return WaitForPlayGamesPlatformThenAuthenticate(async success =>
            {
                if (success)
                {
                    Debug.Log("GPGS login success.");
                    await CHPlaySignInFirebase();
                }
                else
                {
                    Debug.Log("GPGS login failed or no account linked. Creating new user.");
                    // UserManager.Instance.InitializeNewUserData();
                    // NewUser?.Invoke();
                }
            });
        }
        else
        {
            SignInByLocalData(data);
            // OldUser?.Invoke();
        }
    }


    public async Task CHPlaySignInFirebase()
    {
        string userID = PlayGamesPlatform.Instance.GetUserId();
        LocalUserData data = await FirebaseManager.Instance.GetUserDataFromFirebase(userID);
        if (data == null)
        {
           data = UserManager.Instance.InitializeNewUserData(userID);
        }
        else
        {
            UserManager.Instance.ModifyUserID(userID);  
        }
        UserManager.Instance.UserData = data;
    }

    public void HandleChangeScene()
    {
        if (!IsSceneLoaded)
        {
            IsSceneLoaded = true;
            Loader.Load(Loader.Scene.Town);
        }
    }

    public void SignInByLocalData(LocalUserData data)
    {
        UserManager.Instance.UserData = data;

        if (UserManager.Instance.UserData == null)
        {
            Debug.LogError("Local user data is null.");
        }
    }

    #region Link Google Play Games

    public void LinkGooglePlayGamesAccount(Action s = null, Action f = null)
    {
        StartCoroutine(WaitForPlayGamesPlatformThenAuthenticate(success =>
        {
            if (!success)
            {
                Debug.LogWarning("GPGS authentication failed. Cannot link account.");
                return;
            }


            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
            {
                Credential credential = PlayGamesAuthProvider.GetCredential(code);
                LinkAccount(credential,
                    onSuccess: () => s.Invoke(),
                    onFailure: () => f.Invoke());
            });
        }));
    }

    public string GetChPlayID()
    {
        return PlayGamesPlatform.Instance.GetUserId();
    }

    public void LinkAccount(Credential credential, Action onSuccess, Action onFailure = null)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No Firebase user signed in.");
            onFailure?.Invoke();
            return;
        }

        auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Firebase account successfully linked.");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to link Firebase account: " + task.Exception);
                onFailure?.Invoke();
            }
        });
    }

    public async Task<bool> SignInWithPlayGames()
    {
        var tcs = new TaskCompletionSource<bool>();

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Credential credential = PlayGamesAuthProvider.GetCredential(code);
                    auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            // OnAuthenticationSuccessfully?.Invoke(task.Result);
                            tcs.SetResult(true);
                        }
                        else
                        {
                            Debug.LogError("Firebase GPGS sign-in failed: " + task.Exception);
                            tcs.SetResult(false);
                        }
                    });
                });
            }
            else
            {
                Debug.LogError("GPGS auth failed: " + status);
                tcs.SetResult(false);
            }
        });

        return await tcs.Task;
    }

    public IEnumerator WaitForPlayGamesPlatformThenAuthenticate(Action<bool> callback)
    {
        var tcs = new TaskCompletionSource<bool>();
        float timeout = 5f;
        float timer = 0f;

        while (PlayGamesPlatform.Instance == null && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (PlayGamesPlatform.Instance != null)
        {
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status == SignInStatus.Success)
                {
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                    {
                        Credential credential = PlayGamesAuthProvider.GetCredential(code);
                        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted && !task.IsFaulted)
                            {
                                // OnAuthenticationSuccessfully?.Invoke(task.Result);
                                tcs.SetResult(true);
                            }
                            else
                            {
                                Debug.LogError("Firebase GPGS sign-in failed: " + task.Exception);
                                tcs.SetResult(false);
                            }
                        });
                    });
                }

                callback?.Invoke(status == SignInStatus.Success);
            });
        }
        else
        {
            Debug.LogError("PlayGamesPlatform.Instance never initialized.");
            callback?.Invoke(false);
        }
    }

    #endregion
}
#endif