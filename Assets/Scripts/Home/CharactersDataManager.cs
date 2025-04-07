using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using Match3.Enums;


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



    public void LoadDialogueData(LanguageType l)
    {
        characterDialogues = Resources.LoadAll<CharacterDialogueSO>($"DataSO/CharacterDialogues_{l}").ToList();
    }

    private void LoadAllCharactersData() //get from user later
    {
        // foreach (var characterActivity in characterActivities)
        // {
        //     charactersData.Add(new CharactersData(characterActivity));
        // }\
        for (int i = 0; i < characterActivities.Count; i++)
        {
            if (i == 0)
            {
                charactersData.Add(new CharactersData(characterActivities[i], 0));
            }
            else
            {
                charactersData.Add(new CharactersData(characterActivities[i], 30));
            }
        }
    }


    private void LoadDataSo()
    {
        characterActivities = Resources.LoadAll<CharacterActivitySO>("DataSO/CharacterActivities").ToList();
        // characterDialogues = Resources.LoadAll<CharacterDialogueSO>("DataSO/CharacterDialogues").ToList();
        OnCharacterDataLoaded?.Invoke();
    }

    public int TotalHeartPoints()
    {
        int total = 0;
        foreach (var characterData in charactersData)
        {
            total += characterData.currentSympathy;
        }

        return total;
    }

    public CharacterAppearance GetCharacterAppearanceData(CharacterID id)
    {
        return characterColor.AppearancesInfo.Find(x => x.id == id);
    }

    public Color GetHeartColor(int level, out Color nextLevelColor)
    {
        if (level + 1 < characterColor.heartColors.Length)
        {
            nextLevelColor = characterColor.heartColors[level + 1];
        }
        else
        {
            nextLevelColor = characterColor.heartColors[level];
        }

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
            //Debug.Log(characterActivity.id);
            if (characterActivity.dayOff == day) continue;
            if (characterActivity.activityInfos.Any(info => info.dayOfWeek == day))
            {
                Debug.Log($"ADD: {characterActivity.id}");
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
    public int totalSympathyRequired;

    public CharactersData(CharacterActivitySO characterActivity, int t)
    {
        currentSympathy = 0;
        this.characterActivity = characterActivity;
        totalSympathyRequired = t;
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