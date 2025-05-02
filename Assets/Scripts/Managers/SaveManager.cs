using System;
using UnityEngine;
#if !UNITY_WEBGL
using Firebase.Extensions;
using Firebase.Firestore;
#endif
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public const string USER_DATA = "UserData";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void OnApplicationQuit()
    {
#if !UNITY_WEBGL
        SaveAndUploadUserDataToFirebase();
#endif
        // SaveAndUploadShopDataToFirebase();
    }

    private void Start()
    {

    }

    #region USERDATA


#if !UNITY_WEBGL
    public void SaveUserData(UserData userData)
    {
        string json = JsonUtility.ToJson(userData);
        PlayerPrefs.SetString(USER_DATA, json);
        PlayerPrefs.Save();
    }

    public void DeleteUserData()
    {
        UserManager.Instance.UserData = null;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }



    public void SaveAndUploadUserDataToFirebase()
    {
        Debug.Log("Save And Upload User Data To Firebase");
        UserManager.Instance.UserData.LastOnline = FieldValue.ServerTimestamp;
        
        string userID = FirebaseManager.Instance.User.UserId;
        Firebase.Firestore.DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users")
            .Document(userID);
        docRef.SetAsync(UserManager.Instance.UserData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Save complete");
            }
            else
            {
                Debug.LogError("Error creating new user document: " + task.Exception);
            }
        });
    }
#endif
    #endregion
}