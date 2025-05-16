using System;
using UnityEngine;
using System.Collections.Generic;
using Match3.Enums;
using Match3;
using System.Threading.Tasks;

#if !UNITY_WEBGL
using Firebase.Firestore;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }
    public event Action OnUserDataLoaded;
    public event Action<int> OnEnergyChanged;
    public event Action<float> OnGoldChanged;

    private int maxEnergy = 100;
    private const string USER_ID_KEY = "user_ID";

    // public UserData UserData { get; set; }
    public LocalUserData UserData;
    public int TotalHeart => GetTotalHeart();

    private string GenerateUniqueUserID()
    {
        string uniqueID = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString(USER_ID_KEY, uniqueID);
        PlayerPrefs.Save();
        return uniqueID;
    }

    public string GetUserID()
    {
        return PlayerPrefs.GetString(USER_ID_KEY, String.Empty);
    }

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
        // GameDataManager.Instance.OnDataLoaded += InitializeNewUserData;
        GameplayManager.OnWin += OnWinEvent;
        GameplayManager.OnGameOver += OnLoseEvent;

        // Subscribe to minute elapsed event for energy regeneration
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnMinuteElapsed += OnMinuteElapsed;
        }

    }

    private void OnDestroy()
    {
        GameplayManager.OnWin -= OnWinEvent;

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnMinuteElapsed -= OnMinuteElapsed;
        }
    }

    private void OnMinuteElapsed()
    {
        // Regenerate 1 energy per minute if not at max
        if (UserData.Energy < maxEnergy)
        {
            RestoreEnergy(1);
        }
    }

    private void OnWinEvent(CharacterID id, int heart)
    {
        CharacterData data = UserData.AllCharacterData.Find(x => id == x.CharacterID);
        if (data != null)
        {
            data.SetPassLevel(LevelManager.Instance.CurrentLevelIndex, heart);
        }
    }

    private void OnLoseEvent()
    {
        UserData.LoseStreak += 1;
    }

    public LocalUserData InitializeNewUserData()
    {
        Debug.Log("new user");
        GenerateUniqueUserID();
        List<CharacterData> allCharacterData = new List<CharacterData>();
        foreach (CharacterID id in System.Enum.GetValues(typeof(CharacterID)))
        {
            if (GameDataManager.Instance.TryGetCharacterLevelDataByID(id, out CharacterLevelDataV2 characterLevelData))
            {
                //Debug.Log($"id: {id}");
                List<int> stars = new List<int>();

                for (int i = 0; i < characterLevelData.Levels.Count; i++)
                {
                    stars.Add(0);
                }

                CharacterData characterData = new CharacterData()
                {
                    CharacterID = id,
                    Hearts = stars,
                    higestLevel = 0
                };
                allCharacterData.Add(characterData);
            }
        }

        UserData = new LocalUserData()
        {
            AvaiableBoosters = new()
            {
                new BoosterSlot(Match3.BoosterID.ColorBurst, 99),
                new BoosterSlot(Match3.BoosterID.BlastBomb, 99),
                new BoosterSlot(Match3.BoosterID.AxisBomb, 99),
            },
            EquipBooster = new()
            {
                new BoosterSlot(Match3.BoosterID.ExtraMove, 99),
                new BoosterSlot(Match3.BoosterID.FreeSwitch, 99),
                new BoosterSlot(Match3.BoosterID.Hammer, 99),
            },
            AllCharacterData = allCharacterData,
            LastOnlineTimestamp = TimeManager.Instance.LoginTime.ToString(),
            SpinTime = TimeManager.Instance.ServerTime.AddHours(-12).ToString(),
            Energy = 80,
            Gold =300,
            LoseStreak =0
        };

        OnUserDataLoaded?.Invoke();

        return UserData;
    }

    public int GetTotalHeart()
    {
        int totalStar = 0;
        for (int i = 0; i < UserData.AllCharacterData.Count; i++)
        {
            totalStar += UserData.AllCharacterData[i].TotalHeartPoints();
        }

        return totalStar;
    }

    public CharacterData GetCharacterData(CharacterID id)
    {
        return UserData.AllCharacterData.Find(x => x.CharacterID == id);
    }

    public void RestoreEnergy(int amount)
    {
        if (UserData != null)
        {
            int oldEnergy = UserData.Energy;
            UserData.Energy += amount;

            // Only invoke event if energy actually changed
            if (oldEnergy != UserData.Energy)
            {
                Debug.Log($"Energy restored: {oldEnergy} -> {UserData.Energy}");
                OnEnergyChanged?.Invoke(UserData.Energy);
            }
        }
    }

    public void ConsumeEnergy(int amount)
    {
        if (UserData != null)
        {
            int oldEnergy = UserData.Energy;
            UserData.Energy = Mathf.Max(UserData.Energy - amount, 0);

            // Only invoke event if energy actually changed
            if (oldEnergy != UserData.Energy)
            {
                Debug.Log($"Energy consumed: {oldEnergy} -> {UserData.Energy}");
                OnEnergyChanged?.Invoke(UserData.Energy);
            }
        }
    }

    public void ConsumeGold(float amount)
    {
        if (UserData != null)
        {
            float oldGold = UserData.Gold;
            UserData.Gold = Mathf.Max(UserData.Gold - amount, 0);

            // Only invoke event if gold actually changed
            if (oldGold != UserData.Gold)
            {
                Debug.Log($"Gold consumed: {oldGold} -> {UserData.Gold}");
                OnGoldChanged?.Invoke(UserData.Gold);
            }
        }
    }

    // Method to check if there's enough gold before consuming
    public bool HasEnoughGold(float amount)
    {
        return UserData != null && UserData.Gold >= amount;
    }

    // Helper method to add gold
    public void AddGold(float amount)
    {
        if (UserData != null)
        {
            float oldGold = UserData.Gold;
            UserData.Gold += amount;

            // Only invoke event if gold actually changed
            if (oldGold != UserData.Gold)
            {
                Debug.Log($"Gold added: {oldGold} -> {UserData.Gold}");
                OnGoldChanged?.Invoke(UserData.Gold);
            }
        }
    }

    public bool HasEnoughEnergy(int amount)
    {
        return UserData != null && UserData.Energy >= amount;
    }



    public void ClaimDailyReward(BoosterID id, int quantity)
    {
        for (int i = 0; i < UserData.AvaiableBoosters.Count; i++)
        {
            if (UserData.AvaiableBoosters[i].BoosterID == id)
            {
                UserData.AvaiableBoosters[i].Quantity += quantity;
            }
        }

    }
}


[FirestoreData]
[System.Serializable]
public class CharacterData
{
    // Serializable fields for Unity
    [SerializeField] private CharacterID characterId;
    [SerializeField] private List<int> hearts;
    [SerializeField] private int highestLevel;

    // Firebase properties mapped to serializable fields
    [FirestoreProperty]
    public CharacterID CharacterID
    {
        get => characterId;
        set => characterId = value;
    }

    [FirestoreProperty]
    public List<int> Hearts
    {
        get => hearts;
        set => hearts = value;
    }

    [FirestoreProperty]
    public int higestLevel
    {
        get => highestLevel;
        set => highestLevel = value;
    }

    // Parameterless constructor for serialization
    public CharacterData()
    {
        characterId = CharacterID.None;
        hearts = new List<int>();
        highestLevel = 0;
    }

    public void SetPassLevel(int index, int start)
    {
        if (highestLevel == index)
        {
            highestLevel++;
        }

        // Make sure the hearts list is large enough
        while (hearts.Count <= index)
        {
            hearts.Add(0);
        }

        hearts[index] = start;
    }

    // Non-serialized helper method
    public int TotalHeartPoints()
    {
        int totalHeartPoints = 0;
        for (int i = 0; i < hearts.Count; i++)
        {
            totalHeartPoints += hearts[i];
        }

        return totalHeartPoints;
    }
}
#endif