#if !UNITY_WEBGL
using System;
using System.IO;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
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

    private void OnApplicationQuit()
    {
        if (UserManager.Instance == null || UserManager.Instance.UserData == null) return;

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
    
    #if UNITY_ANDROID
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
    #endif

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

    public void SaveAndUploadUserDataToFirebase()
    {
        Debug.Log("Save And Upload User Data To Firebase");

        if (
            UserManager.Instance == null ||
            UserManager.Instance.UserData == null)
        {
            Debug.LogError("Cannot save to Firebase: Required instances are null");
            return;
        }

        LocalUserData local = UserManager.Instance.UserData;
        UserData userData = new UserData
        {
            AvaiableBoosters = local.AvaiableBoosters ?? new List<BoosterSlot>(),
            EquipBooster = local.EquipBooster ?? new List<BoosterSlot>(),
            AllCharacterData = local.AllCharacterData ?? new List<CharacterData>(),
            Energy = local.Energy,
            LastOnline =  FieldValue.ServerTimestamp
        };

        // UserManager.Instance.UserData.LastOnlineTimestamp = FieldValue.ServerTimestamp.ToString();
        string userID = UserManager.Instance.GetUserID();
        Firebase.Firestore.DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users")
            .Document(userID);

        docRef.SetAsync(userData).ContinueWithOnMainThread(task =>
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

        string filename = "UserData.json";
        string filePath = Path.Combine(localSavePath, filename);
        string checksumPath = Path.Combine(localSavePath, "UserData.checksum");
    
        LocalUserData localData = UserManager.Instance.UserData;
        if (Utilities.IsConnectedToInternet())
        {
            localData.LastOnlineTimestamp = TimeManager.Instance.ServerTime.ToString();
        }
        string json = JsonUtility.ToJson(localData, true); // true for pretty print
        string checksum = GenerateChecksum(json);   
        string encryptedJson = EncryptData(json);
        if (encryptedJson == null)
        {
            
        }

        File.WriteAllText(filePath, encryptedJson);
        File.WriteAllText(checksumPath, checksum);

       
    }

    public LocalUserData LoadUserDataFromLocalJson()
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

            // Main file check
            if (!File.Exists(filePath) || !File.Exists(checksumPath))
            {
                Debug.LogWarning("Main save files don't exist or are incomplete");

               
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
              
            }

            // Deserialize the decrypted data
            LocalUserData localData;
            try
            {
                localData = JsonUtility.FromJson<LocalUserData>(decryptedJson);
                if (localData == null)
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
            return localData;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    #endregion
}

[System.Serializable]
public class LocalUserData
{
    public List<BoosterSlot> AvaiableBoosters;
    public List<BoosterSlot> EquipBooster;
    public List<CharacterData> AllCharacterData;
    public int Energy;
    public string LastOnlineTimestamp;
    public string SpinTime;
    

    public LocalUserData()
    {
        
    }

    public void SetSpinTime(DateTime t)
    {
        SpinTime = t.ToString();
    }
}
#endif