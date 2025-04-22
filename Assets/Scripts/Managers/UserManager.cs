using System;
using UnityEngine;
using System.Collections.Generic;
using Match3.Enums;
using Match3;
using Firebase.Firestore;
using System.Threading.Tasks;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }
    public event Action OnUserDataLoaded;

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
    }

    private void Start()
    {
        // GameDataManager.Instance.OnDataLoaded += InitializeNewUserData;
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
                    Hearts = stars
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
            LastOnline =TimeManager.Instance.LoginTime
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
}

[FirestoreData]
[System.Serializable]
public class CharacterData
{
    [FirestoreProperty]
    public CharacterID CharacterID { get; set; }

    [FirestoreProperty]
    public List<int> Hearts { get; set; }

    // Parameterless constructor required by Firestore
    public CharacterData()
    {
        CharacterID = CharacterID.None;
        Hearts = new();
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