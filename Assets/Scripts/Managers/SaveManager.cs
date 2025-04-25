using System;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using Firebase.Extensions;

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
        SaveAndUploadUserDataToFirebase();
        // SaveAndUploadShopDataToFirebase();
    }

    private void Start()
    {

    }

    #region USERDATA



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

    // public void SaveAndUploadShopDataToFirebase()
    // {
    //     string userID = FirebaseManager.Instance.User.UserId;
    //     DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("shops").Document(userID);
    //     docRef.SetAsync(ShopManager.Instance.Market).ContinueWithOnMainThread(task =>
    //     {
    //         if (task.IsCompleted)
    //         {
    //             Debug.Log("Save Shop complete");
    //         }
    //         else
    //         {
    //             Debug.LogError("Error : " + task.Exception);
    //         }
    //     });
    // }

    #endregion
}