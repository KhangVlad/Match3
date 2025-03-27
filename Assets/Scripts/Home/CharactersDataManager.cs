using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharactersDataManager : MonoBehaviour
{
    public static CharactersDataManager Instance { get; private set; }
    public List<CharacterActivitySO> characterActivities = new();
    public CharacterAppearanceSO characterColor;
    public List<CharactersData> charactersData = new List<CharactersData>();
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
        LoadDataSO();
        LoadAllCharactersData();
    }

    private void LoadAllCharactersData() //get from user later
    {
        foreach (var characterActivity in characterActivities)
        {
            charactersData.Add(new CharactersData(characterActivity));
        }
    }

    private void LoadDataSO()
    {
        characterActivities = Resources.LoadAll<CharacterActivitySO>("DataSO/CharacterActivities").ToList();
        OnCharacterDataLoaded?.Invoke();
    }


    public CharacterAppearance GetCharacterAppearanceData(CharacterID id)
    {
        return characterColor.characterAppearances.Find(x => x.id == id);
    }


    public List<CharacterActivitySO> GetCharacterActive(DayInWeek day) //current day
    {
        List<CharacterActivitySO> a = new List<CharacterActivitySO>();
        foreach (var characterActivity in characterActivities)
        {
            if (characterActivity.dayOff == day)
            {
                continue;
            }

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
}