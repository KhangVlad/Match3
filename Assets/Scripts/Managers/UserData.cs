// using UnityEngine;
// using System.Collections.Generic;
// using Firebase.Firestore;
//
// [FirestoreData]
// [System.Serializable]
// public class UserData
// {
//     [FirestoreProperty]
//     public List<BoosterSlot> AvaiableBoosters { get; set; }
//
//     [FirestoreProperty]
//     public List<BoosterSlot> EquipBooster { get; set; }
//
//     [FirestoreProperty]
//     public List<CharacterData> AllCharacterData { get; set; }
//     
//     
//     [FirestoreProperty]
//     public int Energy { get; set; }
//
//
//     [FirestoreProperty] public object LastOnline { get; set; }
//      
//     [FirestoreProperty]
//     public bool DailyRewardFlag { get; set; } 
//
// }

#if !UNITY_WEBGL
using UnityEngine;
using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
[System.Serializable]
public class UserData
{
    // Serializable fields for Unity
    [SerializeField] private List<BoosterSlot> avaiableBoosters;
    [SerializeField] private List<BoosterSlot> equipBooster;
    [SerializeField] private List<CharacterData> allCharacterData;
    [SerializeField] private int energy;
    [SerializeField] private bool dailyRewardFlag;

    // Non-serialized field for Firebase timestamp
    [System.NonSerialized] private object lastOnline;


    [System.NonSerialized] private object lastSpinTime;


    // Firebase properties mapped to serializable fields
    [FirestoreProperty]
    public List<BoosterSlot> AvaiableBoosters
    {
        get => avaiableBoosters;
        set => avaiableBoosters = value;
    }

    [FirestoreProperty]
    public List<BoosterSlot> EquipBooster
    {
        get => equipBooster;
        set => equipBooster = value;
    }

    [FirestoreProperty]
    public List<CharacterData> AllCharacterData
    {
        get => allCharacterData;
        set => allCharacterData = value;
    }

    [FirestoreProperty]
    public int Energy
    {
        get => energy;
        set
        {
            if (value > 100)
                energy = 100;
            else if (value < 0)
                energy = 0;
            else
                energy = value;
        }
    }

    [FirestoreProperty]
    public object LastOnline
    {
        get => lastOnline;
        set => lastOnline = value;
    }

    [FirestoreProperty]
    public object LastSpinTime
    {
        get => lastSpinTime;
        set => lastSpinTime = value;
    }

    [FirestoreProperty]
    public bool DailyRewardFlag
    {
        get => dailyRewardFlag;
        set => dailyRewardFlag = value;
    }


    // Constructor to initialize lists
    public UserData()
    {
        avaiableBoosters = new List<BoosterSlot>();
        equipBooster = new List<BoosterSlot>();
        allCharacterData = new List<CharacterData>();
        energy = 0;
        dailyRewardFlag = false;
    }
}
#endif