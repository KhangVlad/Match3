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
    public event Action<int> OnEnergyChanged; // New event for energy changes

     private int maxEnergy = 100; // Maximum energy cap
    
    public UserData UserData { get; set; }
    public int TotalHeart => GetTotalHeart();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // GameDataManager.Instance.OnDataLoaded += InitializeNewUserData;
        GameplayManager.OnWin += OnWinEvent;
        
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
    
    public UserData InitializeNewUserData()
    {
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

        UserData = new UserData()
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
            LastOnline = TimeManager.Instance.LoginTime,
            Energy = 80,
            DailyRewardFlag = false,
        };
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
            UserData.Energy = Mathf.Min(UserData.Energy + amount, maxEnergy);
            
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
    
    public bool HasEnoughEnergy(int amount)
    {
        return UserData != null && UserData.Energy >= amount;
    }

    public void ResetDailyGift()
    {
        UserData.DailyRewardFlag = false;
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
        UserData.DailyRewardFlag = true;
    }

    public bool IsAvailableDailyGift => !UserData.DailyRewardFlag;
}


[FirestoreData]
[System.Serializable]
public class CharacterData
{
    [FirestoreProperty]
    public CharacterID CharacterID { get; set; }

    [FirestoreProperty]
    public List<int> Hearts { get; set; }
    
    [FirestoreProperty]
    public int higestLevel { get; set; }

    // Parameterless constructor required by Firestore
    public CharacterData()
    {
        CharacterID = CharacterID.None;
        Hearts = new();
        higestLevel = 0;
    }

    public void SetPassLevel(int index, int start)
    {
        if (higestLevel == index)
        {
            higestLevel++;
        }
        this.Hearts[index] = start;
    }

    // Non-serialized helper method
    public int TotalHeartPoints()
    {
        int totalHeartPoints = 0;
        for (int i = 0; i < Hearts.Count; i++)
        {
            totalHeartPoints += Hearts[i];
        }
        return totalHeartPoints;
    }
}
#endif