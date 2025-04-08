using System;
using UnityEngine;
using System.Collections.Generic;
using Match3.Enums;
using Match3;
using UnityEngine.Serialization;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }
  

    [SerializeField] private UserData _userData;
    public event Action OnUserDataLoaded;

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
        GameDataManager.Instance.OnDataLoaded += InitializeNewUserData;
    }

    private void InitializeNewUserData()
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

        _userData = new UserData()
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
            AllCharacterData = allCharacterData
        };
        OnUserDataLoaded?.Invoke();
       
    }

    public int GetTotalHeart()
    {
        int totalStar = 0;
       for( int i=0; i< _userData.AllCharacterData.Count; i++)
        {
            totalStar += _userData.AllCharacterData[i].TotalHeartPoints();
        }

        return totalStar;
    }
    
    public CharacterData GetCharacterData(CharacterID id)
    {
        return _userData.AllCharacterData.Find(x => x.CharacterID == id);
    }
    
    public int GetTotalHeartCharsOfChar( CharacterID id)
    {
        CharacterData characterData = GetCharacterData(id);
        if (characterData == null)
        {
            Debug.LogError($"Character data not found for ID: {id}");
            return 0;
        }
        return characterData.TotalHeartPoints();
    }


    
}

[System.Serializable]
public class CharacterData
{
    public CharacterID CharacterID;
    public List<int> Hearts;
   

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