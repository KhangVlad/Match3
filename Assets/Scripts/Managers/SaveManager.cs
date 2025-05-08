// using System;
// using System.IO;
// using UnityEngine;
// using Firebase.Firestore;
// using System.Threading.Tasks;
// using Firebase.Extensions;
// using System.Linq;
// using Match3.Shares;
// using System.Collections.Generic;
//
// public class SaveManager : MonoBehaviour
// {
//     public static SaveManager Instance { get; private set; }
//     public const string USER_DATA = "UserData";
//     private string localSavePath;
//
//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(this.gameObject);
//             return;
//         }
//
//         Instance = this;
//         localSavePath = Path.Combine(Application.persistentDataPath, "LocalSaves");
//
//         // Create directory if it doesn't exist
//         if (!Directory.Exists(localSavePath))
//         {
//             Directory.CreateDirectory(localSavePath);
//         }
//     }
//
//     private void OnApplicationQuit()
//     {
//         if (Utilities.IsConnectedToInternet())
//         {
//             SaveAndUploadUserDataToFirebase();
//             SaveUserDataToLocalJson();
//         }
//         else
//         {
//             SaveUserDataToLocalJson();
//         }
//
//         // SaveAndUploadShopDataToFirebase();
//     }
//
//
//     private void OnApplicationFocus(bool focusStatus)
//     {
//         #if UNITY_ANDROID && !UNITY_EDITOR
//         if (UserManager.Instance.UserData == null) return;
//         if (Utilities.IsConnectedToInternet())
//         {
//             SaveAndUploadUserDataToFirebase();
//             SaveUserDataToLocalJson();
//         }
//         else
//         {
//             SaveUserDataToLocalJson();
//         }
//         #endif
//     }
//
//
//     private void Start()
//     {
//     }
//
//     #region USERDATA
//
//     public void SaveUserData(UserData userData)
//     {
//         string json = JsonUtility.ToJson(userData);
//         PlayerPrefs.SetString(USER_DATA, json);
//         PlayerPrefs.Save();
//     }
//
//     public void DeleteUserData()
//     {
//         UserManager.Instance.UserData = null;
//         PlayerPrefs.DeleteAll();
//         PlayerPrefs.Save();
//     }
//
//
//     public void SaveAndUploadUserDataToFirebase()
//     {
//         Debug.Log("Save And Upload User Data To Firebase");
//         UserManager.Instance.UserData.LastOnline = FieldValue.ServerTimestamp;
//         string userID = FirebaseManager.Instance.User.UserId;
//         Firebase.Firestore.DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users")
//             .Document(userID);
//         docRef.SetAsync(UserManager.Instance.UserData).ContinueWithOnMainThread(task =>
//         {
//             if (task.IsCompleted)
//             {
//                 Debug.Log("Save complete");
//             }
//             else
//             {
//                 Debug.LogError("Error creating new user document: " + task.Exception);
//             }
//         });
//     }
//
//
//     private void SaveUserDataToLocalJson()
//     {
//         if (UserManager.Instance.UserData == null)
//         {
//             Debug.LogWarning("Cannot save user data to JSON: UserData is null");
//             return;
//         }
//
//         try
//         {
//             string filename = $"UserData.json";
//             string filePath = Path.Combine(localSavePath, filename);
//             if (UserManager.Instance.UserData.AvaiableBoosters == null)
//                 UserManager.Instance.UserData.AvaiableBoosters = new List<BoosterSlot>();
//             if (UserManager.Instance.UserData.EquipBooster == null)
//                 UserManager.Instance.UserData.EquipBooster = new List<BoosterSlot>();
//             if (UserManager.Instance.UserData.AllCharacterData == null)
//                 UserManager.Instance.UserData.AllCharacterData = new List<CharacterData>();
//
//             SerializableUserData serializableData = new SerializableUserData(UserManager.Instance.UserData);
//
//             // Convert to JSON
//             string json = JsonUtility.ToJson(serializableData, true); // true for pretty print
//             Debug.Log(json);
//
//             // Write to file
//             File.WriteAllText(filePath, json);
//
//             Debug.Log($"Successfully saved user data to local JSON: {filePath}");
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"Failed to save user data to local JSON: {ex.Message}");
//             Debug.LogException(ex); // Add this to see the stack trace
//         }
//     }
//
//     public UserData LoadUserDataFromLocalJson()
//     {
//         try
//         {
//             // Check if directory exists
//             if (!Directory.Exists(localSavePath))
//             {
//                 Debug.LogWarning($"Local save directory does not exist: {localSavePath}");
//                 return null;
//             }
//
//             // Path for the single UserData.json file (matching what SaveUserDataToLocalJSON uses)
//             string filePath = Path.Combine(localSavePath, "UserData.json");
//
//             // Verify file exists and has content
//             if (!File.Exists(filePath))
//             {
//                 Debug.LogWarning($"UserData.json file doesn't exist at: {filePath}");
//                 return null;
//             }
//
//             long fileSize = new FileInfo(filePath).Length;
//
//             if (fileSize == 0)
//             {
//                 return null;
//             }
//
//             string json = File.ReadAllText(filePath);
//             if (string.IsNullOrWhiteSpace(json) || json == "{}")
//             {
//                 return null;
//             }
//
//             // Try deserializing
//             SerializableUserData serializableData;
//             try
//             {
//                 serializableData = JsonUtility.FromJson<SerializableUserData>(json);
//
//                 if (serializableData == null)
//                 {
//                     return null;
//                 }
//             }
//             catch (Exception e)
//             {
//                 return null;
//             }
//
//
//             UserData userData = new UserData
//             {
//                 AvaiableBoosters = serializableData.avaiableBoosters ?? new List<BoosterSlot>(),
//                 EquipBooster = serializableData.equipBooster ?? new List<BoosterSlot>(),
//                 AllCharacterData = serializableData.allCharacterData ?? new List<CharacterData>(),
//                 Energy = serializableData.energy,
//                 DailyRewardFlag = serializableData.dailyRewardFlag,
//                 LastOnline = serializableData.lastOnlineTimestamp
//             };
//
//             return userData;
//         }
//         catch (Exception ex)
//         {
//             Debug.LogException(ex);
//             return null;
//         }
//     }
//
//
//     // public void SaveAndUploadShopDataToFirebase()
//     // {
//     //     string userID = FirebaseManager.Instance.User.UserId;
//     //     DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("shops").Document(userID);
//     //     docRef.SetAsync(ShopManager.Instance.Market).ContinueWithOnMainThread(task =>
//     //     {
//     //         if (task.IsCompleted)
//     //         {
//     //             Debug.Log("Save Shop complete");
//     //         }
//     //         else
//     //         {
//     //             Debug.LogError("Error : " + task.Exception);
//     //         }
//     //     });
//     // }
//
//     #endregion
// }
//
// [System.Serializable]
// public class SerializableUserData
// {
//     public List<BoosterSlot> avaiableBoosters;
//     public List<BoosterSlot> equipBooster;
//     public List<CharacterData> allCharacterData;
//     public int energy;
//     public bool dailyRewardFlag;
//     public string lastOnlineTimestamp; // Store timestamp as string for local save
//
//     // Constructor to convert from UserData
//     public SerializableUserData(UserData userData)
//     {
//         avaiableBoosters = userData.AvaiableBoosters;
//         equipBooster = userData.EquipBooster;
//         allCharacterData = userData.AllCharacterData;
//         energy = userData.Energy;
//         dailyRewardFlag = userData.DailyRewardFlag;
//         lastOnlineTimestamp = DateTime.Now.ToString();
//     }
//
//     public SerializableUserData()
//     {
//         // Initialize collections to prevent null reference exceptions
//         avaiableBoosters = new List<BoosterSlot>();
//         equipBooster = new List<BoosterSlot>();
//         allCharacterData = new List<CharacterData>();
//         lastOnlineTimestamp = "";
//     }
// }

using System;
using System.IO;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using Firebase.Extensions;
using System.Linq;
using Match3.Shares;
using System.Collections.Generic;
using System.Security.Cryptography;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public const string USER_DATA = "UserData";
    private string localSavePath;
    private readonly string encryptionKey = "Match3GameSecretKey2025"; // Change this to something unique

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

    // private void OnApplicationQuit()
    // {
    //     if (UserManager.Instance == null || UserManager.Instance.UserData == null) return;
    //     
    //     if (Utilities.IsConnectedToInternet())
    //     {
    //         SaveAndUploadUserDataToFirebase();
    //         SaveUserDataToLocalJson();
    //     }
    //     else
    //     {
    //         SaveUserDataToLocalJson();
    //     }
    // }

    // private void OnApplicationFocus(bool focusStatus)
    // {
    //     // #if UNITY_ANDROID && !UNITY_EDITOR
    //     if (UserManager.Instance == null || UserManager.Instance.UserData == null) return;
    //     
    //     if (!focusStatus) // Save when app loses focus
    //     {
    //         if (Utilities.IsConnectedToInternet())
    //         {
    //             SaveAndUploadUserDataToFirebase();
    //             SaveUserDataToLocalJson();
    //         }
    //         else
    //         {
    //             SaveUserDataToLocalJson();
    //         }
    //     }
    //     // #endif
    // }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) // Save when app is paused
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
    }

    #region Encryption and Checksum Methods

    private string EncryptData(string data)
    {
        try
        {
            // Convert the data and key to bytes
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(encryptionKey);
            
            // Create a hash of the key for better security
            using (var md5 = MD5.Create())
            {
                keyBytes = md5.ComputeHash(keyBytes);
            }
            
            // Encrypt the data using XOR
            byte[] encryptedBytes = new byte[dataBytes.Length];
            for (int i = 0; i < dataBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            
            // Convert to Base64 string for safe storage
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Encryption failed: {ex.Message}");
            return null;
        }
    }

    private string DecryptData(string encryptedData)
    {
        try
        {
            // Convert the encrypted data from Base64
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(encryptionKey);
            
            // Create a hash of the key
            using (var md5 = MD5.Create())
            {
                keyBytes = md5.ComputeHash(keyBytes);
            }
            
            // Decrypt the data (XOR is reversible)
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            
            // Convert back to string
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Decryption failed: {ex.Message}");
            return null;
        }
    }

    private string GenerateChecksum(string data)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    private bool VerifyChecksum(string data, string storedChecksum)
    {
        string calculatedChecksum = GenerateChecksum(data);
        return calculatedChecksum == storedChecksum;
    }

    #endregion

    #region UserData Operations

    public void SaveUserData(UserData userData)
    {
        string json = JsonUtility.ToJson(userData);
        PlayerPrefs.SetString(USER_DATA, json);
        PlayerPrefs.Save();
    }

    public void DeleteUserData()
    {
        // Clear memory
        UserManager.Instance.UserData = null;
        
        // Clear PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // Clear local JSON files
        try
        {
            string filePath = Path.Combine(localSavePath, "UserData.json");
            string checksumPath = Path.Combine(localSavePath, "UserData.checksum");
            string backupPath = Path.Combine(localSavePath, "UserData.json.backup");
            
            if (File.Exists(filePath))
                File.Delete(filePath);
                
            if (File.Exists(checksumPath))
                File.Delete(checksumPath);
                
            if (File.Exists(backupPath))
                File.Delete(backupPath);
                
            Debug.Log("Local save files deleted successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting local save files: {ex.Message}");
        }
    }

    public void SaveAndUploadUserDataToFirebase()
    {
        Debug.Log("Save And Upload User Data To Firebase");
        
        if (FirebaseManager.Instance == null || 
            FirebaseManager.Instance.User == null || 
            UserManager.Instance == null || 
            UserManager.Instance.UserData == null)
        {
            Debug.LogError("Cannot save to Firebase: Required instances are null");
            return;
        }
        
        UserManager.Instance.UserData.LastOnline = FieldValue.ServerTimestamp;
        string userID = FirebaseManager.Instance.User.UserId;
        Firebase.Firestore.DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users")
            .Document(userID);
            
        docRef.SetAsync(UserManager.Instance.UserData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Save to Firebase complete");
            }
            else
            {
                Debug.LogError("Error saving user document to Firebase: " + task.Exception);
            }
        });
    }

    private void SaveUserDataToLocalJson()
    {
        if (UserManager.Instance == null || UserManager.Instance.UserData == null)
        {
            Debug.LogWarning("Cannot save user data to JSON: UserData is null");
            return;
        }

        try
        {
            string filename = "UserData.json";
            string filePath = Path.Combine(localSavePath, filename);
            string checksumPath = Path.Combine(localSavePath, "UserData.checksum");
            string backupPath = Path.Combine(localSavePath, "UserData.json.backup");
            
            // Create backup of existing file if it exists
            if (File.Exists(filePath))
            {
                File.Copy(filePath, backupPath, true);
            }
            
            // Ensure collections are initialized
            if (UserManager.Instance.UserData.AvaiableBoosters == null)
                UserManager.Instance.UserData.AvaiableBoosters = new List<BoosterSlot>();
            if (UserManager.Instance.UserData.EquipBooster == null)
                UserManager.Instance.UserData.EquipBooster = new List<BoosterSlot>();
            if (UserManager.Instance.UserData.AllCharacterData == null)
                UserManager.Instance.UserData.AllCharacterData = new List<CharacterData>();

            // Create serializable version
            SerializableUserData serializableData = new SerializableUserData(UserManager.Instance.UserData);

            // Convert to JSON
            string json = JsonUtility.ToJson(serializableData, true); // true for pretty print
            
            // Generate checksum
            string checksum = GenerateChecksum(json);
            
            // Encrypt the JSON data
            string encryptedJson = EncryptData(json);
            if (encryptedJson == null)
            {
                throw new Exception("Encryption failed");
            }
            
            // Write encrypted data to file
            File.WriteAllText(filePath, encryptedJson);
            
            // Write checksum to separate file
            File.WriteAllText(checksumPath, checksum);

            Debug.Log($"Successfully saved encrypted user data to local JSON with checksum verification");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save user data to local JSON: {ex.Message}");
            Debug.LogException(ex);
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

            string filePath = Path.Combine(localSavePath, "UserData.json");
            string checksumPath = Path.Combine(localSavePath, "UserData.checksum");
            string backupPath = Path.Combine(localSavePath, "UserData.json.backup");

            // Main file check
            if (!File.Exists(filePath) || !File.Exists(checksumPath))
            {
                Debug.LogWarning("Main save files don't exist or are incomplete");
                
                // Try backup if main file doesn't exist
                if (File.Exists(backupPath))
                {
                    Debug.Log("Attempting to restore from backup");
                    File.Copy(backupPath, filePath, true);
                }
                else
                {
                    return null;
                }
            }

            // Read encrypted data and stored checksum
            string encryptedJson = File.ReadAllText(filePath);
            string storedChecksum = File.ReadAllText(checksumPath);
            
            if (string.IsNullOrWhiteSpace(encryptedJson) || string.IsNullOrWhiteSpace(storedChecksum))
            {
                Debug.LogWarning("UserData files are empty");
                return null;
            }

            // Decrypt the data
            string decryptedJson = DecryptData(encryptedJson);
            if (decryptedJson == null)
            {
                Debug.LogError("Failed to decrypt user data");
                return null;
            }
            
            // Verify checksum
            if (!VerifyChecksum(decryptedJson, storedChecksum))
            {
                Debug.LogError("Checksum verification failed - data may have been tampered with");
                
                // Try to restore from backup
                if (File.Exists(backupPath))
                {
                    Debug.Log("Attempting to restore from backup after checksum failure");
                    File.Copy(backupPath, filePath, true);
                    encryptedJson = File.ReadAllText(filePath);
                    decryptedJson = DecryptData(encryptedJson);
                    
                   if (decryptedJson == null)
                    {
                        Debug.LogError("Failed to decrypt backup user data");
                        return null;
                    }
                    
                    // We don't have a backup of the checksum, so regenerate it
                    storedChecksum = GenerateChecksum(decryptedJson);
                    File.WriteAllText(checksumPath, storedChecksum);
                }
                else
                {
                    return null;
                }
            }

            // Deserialize the decrypted data
            SerializableUserData serializableData;
            try
            {
                serializableData = JsonUtility.FromJson<SerializableUserData>(decryptedJson);
                if (serializableData == null)
                {
                    Debug.LogError("Deserialization resulted in null data");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize user data: {e.Message}");
                Debug.LogException(e);
                return null;
            }

            // Convert to UserData
            UserData userData = new UserData
            {
                AvaiableBoosters = serializableData.avaiableBoosters ?? new List<BoosterSlot>(),
                EquipBooster = serializableData.equipBooster ?? new List<BoosterSlot>(),
                AllCharacterData = serializableData.allCharacterData ?? new List<CharacterData>(),
                Energy = serializableData.energy,
                DailyRewardFlag = serializableData.dailyRewardFlag,
                LastOnline = serializableData.lastOnlineTimestamp
            };

            Debug.Log("Successfully loaded and verified user data");
            return userData;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading user data from local JSON");
            Debug.LogException(ex);
            return null;
        }
    }

    // Optional: Additional utility methods

    // Check if local save exists
    public bool LocalSaveExists()
    {
        string filePath = Path.Combine(localSavePath, "UserData.json");
        string checksumPath = Path.Combine(localSavePath, "UserData.checksum");
        return File.Exists(filePath) && File.Exists(checksumPath);
    }

    // Get last modified time of save file
    public DateTime GetLastSaveTime()
    {
        try
        {
            string filePath = Path.Combine(localSavePath, "UserData.json");
            if (File.Exists(filePath))
            {
                return File.GetLastWriteTime(filePath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting last save time: {ex.Message}");
        }
        return DateTime.MinValue;
    }

    // Create an emergency backup
    public void CreateBackup(string backupName)
    {
        try
        {
            string sourceFile = Path.Combine(localSavePath, "UserData.json");
            string sourceChecksum = Path.Combine(localSavePath, "UserData.checksum");
            
            if (!File.Exists(sourceFile) || !File.Exists(sourceChecksum))
            {
                Debug.LogWarning("Cannot create backup: save files not found");
                return;
            }
            
            string backupFile = Path.Combine(localSavePath, $"UserData_{backupName}.json");
            string backupChecksum = Path.Combine(localSavePath, $"UserData_{backupName}.checksum");
            
            File.Copy(sourceFile, backupFile, true);
            File.Copy(sourceChecksum, backupChecksum, true);
            
            Debug.Log($"Created backup: {backupName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create backup: {ex.Message}");
        }
    }

    // Restore from a named backup
    public bool RestoreFromBackup(string backupName)
    {
        try
        {
            string backupFile = Path.Combine(localSavePath, $"UserData_{backupName}.json");
            string backupChecksum = Path.Combine(localSavePath, $"UserData_{backupName}.checksum");
            
            if (!File.Exists(backupFile) || !File.Exists(backupChecksum))
            {
                Debug.LogWarning($"Backup {backupName} not found");
                return false;
            }
            
            string targetFile = Path.Combine(localSavePath, "UserData.json");
            string targetChecksum = Path.Combine(localSavePath, "UserData.checksum");
            
            File.Copy(backupFile, targetFile, true);
            File.Copy(backupChecksum, targetChecksum, true);
            
            Debug.Log($"Restored from backup: {backupName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to restore from backup: {ex.Message}");
            return false;
        }
    }

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
    public int saveVersion = 1; // For future data migrations

    // Constructor to convert from UserData
    public SerializableUserData(UserData userData)
    {
        avaiableBoosters = userData.AvaiableBoosters;
        equipBooster = userData.EquipBooster;
        allCharacterData = userData.AllCharacterData;
        energy = userData.Energy;
        dailyRewardFlag = userData.DailyRewardFlag;
        
        // Store current time as the last online timestamp
        lastOnlineTimestamp = DateTime.Now.ToString("o"); // ISO 8601 format for better parsing
    }

    public SerializableUserData()
    {
        // Initialize collections to prevent null reference exceptions
        avaiableBoosters = new List<BoosterSlot>();
        equipBooster = new List<BoosterSlot>();
        allCharacterData = new List<CharacterData>();
        lastOnlineTimestamp = DateTime.Now.ToString("o");
    }
}