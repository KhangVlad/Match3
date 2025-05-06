using System;
using System.IO;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using Firebase.Extensions;
using System.Linq;
using Match3.Shares;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public const string USER_DATA = "UserData";

    private string localSavePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        localSavePath = Path.Combine(Application.persistentDataPath, "LocalSaves");

        // Create directory if it doesn't exist
        if (!Directory.Exists(localSavePath))
        {
            Directory.CreateDirectory(localSavePath);
        }
    }

    private void OnApplicationQuit()
    {
        if (Utilities.IsConnectedToInternet())
        {
            SaveAndUploadUserDataToFirebase();
            SaveUserDataToLocalJson();
        }
        else
        {
            SaveUserDataToLocalJson();
        }

        // SaveAndUploadShopDataToFirebase();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (Utilities.IsConnectedToInternet())
        {
            SaveAndUploadUserDataToFirebase();
            SaveUserDataToLocalJson();
        }
        else
        {
            SaveUserDataToLocalJson();
        }
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


    private void SaveUserDataToLocalJson()
    {
        if (UserManager.Instance.UserData == null)
        {
            Debug.LogWarning("Cannot save user data to JSON: UserData is null");
            return;
        }
        try
        {
            string filename = $"UserData.json";
            string filePath = Path.Combine(localSavePath, filename);
            if (UserManager.Instance.UserData.AvaiableBoosters == null)
                UserManager.Instance.UserData.AvaiableBoosters = new List<BoosterSlot>();
            if (UserManager.Instance.UserData.EquipBooster == null)
                UserManager.Instance.UserData.EquipBooster = new List<BoosterSlot>();
            if (UserManager.Instance.UserData.AllCharacterData == null)
                UserManager.Instance.UserData.AllCharacterData = new List<CharacterData>();
            
            SerializableUserData serializableData = new SerializableUserData(UserManager.Instance.UserData);

            // Convert to JSON
            string json = JsonUtility.ToJson(serializableData, true); // true for pretty print
            Debug.Log(json);

            // Write to file
            File.WriteAllText(filePath, json);

            Debug.Log($"Successfully saved user data to local JSON: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save user data to local JSON: {ex.Message}");
            Debug.LogException(ex); // Add this to see the stack trace
        }
    }

   public UserData LoadUserDataFromLocalJson()
{
    try
    {
        // Check if directory exists
        if (!Directory.Exists(localSavePath))
        {
            Debug.LogWarning($"Local save directory does not exist: {localSavePath}");
            return null;
        }

        // Path for the single UserData.json file (matching what SaveUserDataToLocalJSON uses)
        string filePath = Path.Combine(localSavePath, "UserData.json");
        
        // Verify file exists and has content
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"UserData.json file doesn't exist at: {filePath}");
            return null;
        }

        long fileSize = new FileInfo(filePath).Length;

        if (fileSize == 0)
        {
            return null;
        }
    
        string json = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return null;
        }

        // Try deserializing
        SerializableUserData serializableData;
        try
        {
            serializableData = JsonUtility.FromJson<SerializableUserData>(json);

            if (serializableData == null)
            {
                return null;
            }
        }
        catch (Exception e)
        {
            return null;
        }


        UserData userData = new UserData
        {
            AvaiableBoosters = serializableData.avaiableBoosters ?? new List<BoosterSlot>(),
            EquipBooster = serializableData.equipBooster ?? new List<BoosterSlot>(),
            AllCharacterData = serializableData.allCharacterData ?? new List<CharacterData>(),
            Energy = serializableData.energy,
            DailyRewardFlag = serializableData.dailyRewardFlag,
            LastOnline = serializableData.lastOnlineTimestamp
        };

        return userData;
    }
    catch (Exception ex)
    {
        Debug.LogException(ex);
        return null;
    }
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

[System.Serializable]
public class SerializableUserData
{
    public List<BoosterSlot> avaiableBoosters;
    public List<BoosterSlot> equipBooster;
    public List<CharacterData> allCharacterData;
    public int energy;
    public bool dailyRewardFlag;
    public string lastOnlineTimestamp; // Store timestamp as string for local save

    // Constructor to convert from UserData
    public SerializableUserData(UserData userData)
    {
        avaiableBoosters = userData.AvaiableBoosters;
        equipBooster = userData.EquipBooster;
        allCharacterData = userData.AllCharacterData;
        energy = userData.Energy;   
        dailyRewardFlag = userData.DailyRewardFlag;
        lastOnlineTimestamp = DateTime.Now.ToString();
    }

    public SerializableUserData()
    {
        // Initialize collections to prevent null reference exceptions
        avaiableBoosters = new List<BoosterSlot>();
        equipBooster = new List<BoosterSlot>();
        allCharacterData = new List<CharacterData>();
        lastOnlineTimestamp = "";
    }
}