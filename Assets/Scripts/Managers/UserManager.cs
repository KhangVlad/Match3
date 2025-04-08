using UnityEngine;
using System.Collections.Generic;
using Match3.Enums;
using Match3;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }

    [SerializeField] private UserData _userData;

    public int TotalStar => GetTotalStar();


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
        InitializeNewUserData();
    }


    private UserData InitializeNewUserData()
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
                    Stars = stars
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

        return null;
    }

    private int GetTotalStar()
    {
        int totalStar = 0;
        for (int i = 0; i < _userData.AllCharacterData.Count; i++)
        {
            CharacterData characterLevelData = _userData.AllCharacterData[i];
            for (int j = 0; j < characterLevelData.Stars.Count; i++)
            {
                totalStar += characterLevelData.Stars[j];
            }
        }
        return totalStar;
    }
}

[System.Serializable]
public class CharacterData
{
    public CharacterID CharacterID;
    public List<int> Stars;
}






