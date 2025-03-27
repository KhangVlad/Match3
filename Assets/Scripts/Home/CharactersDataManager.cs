using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharactersDataManager : MonoBehaviour
{
    public static CharactersDataManager Instance { get; private set; }
    public List<CharacterActivitySO> characterActivities = new();
    public CharacterAppearanceSO characterColor;
    public List<CharactersData> charactersData = new();
    public List<CharacterDialogueSO> characterDialogues = new();
    public event Action OnCharacterDataLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        LoadDataSo();
        LoadAllCharactersData();
    }

    private void LoadAllCharactersData() //get from user later
    {
        foreach (var characterActivity in characterActivities)
        {
            charactersData.Add(new CharactersData(characterActivity));
        }
    }
    
    

    private void LoadDataSo()
    {
        characterActivities = Resources.LoadAll<CharacterActivitySO>("DataSO/CharacterActivities").ToList();
        characterDialogues = Resources.LoadAll<CharacterDialogueSO>("DataSO/CharacterDialogues").ToList();
        OnCharacterDataLoaded?.Invoke();
    }


    public CharacterAppearance GetCharacterAppearanceData(CharacterID id)
    {
        return characterColor.AppearancesInfo.Find(x => x.id == id);
    }

    public Color GetHeartColor(int level, out Color nextLevelColor)
    {
        nextLevelColor = characterColor.heartColors[level + 1];
        return characterColor.heartColors[level];
    }

    public CharactersData GetCharacterData(CharacterID id)
    {
        return charactersData.Find(x => x.characterActivity.id == id);
    }
    
    
    public CharacterDialogueSO GetCharacterDialogue(CharacterID id)
    {
        return characterDialogues.Find(x => x.id == id);
    }


    public List<CharacterActivitySO> GetCharacterActive(DayInWeek day) //current day
    {
        List<CharacterActivitySO> a = new List<CharacterActivitySO>();
        foreach (var characterActivity in characterActivities)
        {
            if (characterActivity.dayOff == day) continue;
            if (characterActivity.activityInfos.Any(info => info.dayOfWeek == day))
            {
                a.Add(characterActivity);
            }
        }

        return a;
    }
}

[Serializable]
public class CharactersData
{
    public int currentSympathy;
    public CharacterActivitySO characterActivity;

    public CharactersData(CharacterActivitySO characterActivity)
    {
        currentSympathy = 0;
        this.characterActivity = characterActivity;
    }

    public int GetLevel()
    {
        for (int i = 0; i < characterActivity.sympathyRequired.Length; i++)
        {
            if (currentSympathy < characterActivity.sympathyRequired[i])
            {
                return i;
            }
        }

        return characterActivity.sympathyRequired.Length;
    }

    public int GetNextLevelSympathy()
    {
        for (int i = 0; i < characterActivity.sympathyRequired.Length; i++)
        {
            if (currentSympathy < characterActivity.sympathyRequired[i])
            {
                return characterActivity.sympathyRequired[i];
            }
        }

        return 0;
    }

    public void IncreaseSympathy(int value)
    {
        currentSympathy += value;
    }
}