﻿#if !UNITY_WEBGL
using UnityEngine;
using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
[System.Serializable]
public class UserData
{
    [FirestoreProperty] public List<BoosterSlot> AvaiableBoosters { get; set; }

    [FirestoreProperty] public List<BoosterSlot> EquipBooster { get; set; }

    [FirestoreProperty] public List<CharacterData> AllCharacterData { get; set; }


    [FirestoreProperty] public int Energy { get; set; }


    [FirestoreProperty] public object LastOnline { get; set; }

    [FirestoreProperty] public string LastSpinTime { get; set; }
    [FirestoreProperty] public float Gold { get; set; }
    [FirestoreProperty] public bool IsBuyWelcomePack { get; set; }
    [FirestoreProperty] public string LinkedCredential { get; set; }
}
#endif
