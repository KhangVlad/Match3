using UnityEngine;
using System.Collections.Generic;
using Firebase.Firestore;

[System.Serializable]
public class UserData
{
    [FirestoreProperty]
    public List<BoosterSlot> AvaiableBoosters { get; set; }

    [FirestoreProperty]
    public List<BoosterSlot> EquipBooster { get; set; }

    [FirestoreProperty]
    public List<CharacterData> AllCharacterData { get; set; }


    [FirestoreProperty] public object LastOnline { get; set; }
}