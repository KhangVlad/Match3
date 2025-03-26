using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharactersDataManager : MonoBehaviour
{
    public static CharactersDataManager Instance { get; private set; }
    [Header("~Runtime")] 
    public List<CharacterActivitySO> characterActivities = new();
    public CharacterAppearanceSO characterColor;

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
    }

    private void LoadDataSO()
    {
        characterActivities = Resources.LoadAll<CharacterActivitySO>("DataSO/CharacterActivities").ToList();
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
public class CharsWithIndex
{
    public CharacterActivitySO characterActivity;
    public int index;
}