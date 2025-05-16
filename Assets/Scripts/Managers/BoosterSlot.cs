// using Match3;
// using Firebase.Firestore;
//
// [FirestoreData]
// [System.Serializable]
// public class BoosterSlot
// {
//     [FirestoreProperty]
//     public BoosterID BoosterID { get; set; }
//
//     [FirestoreProperty]
//     public int Quantity { get; set; }
//
//     // Parameterless constructor required by Firestore
//     public BoosterSlot() {}
//
//     public BoosterSlot(BoosterID boosterID, int quantity)
//     {
//         BoosterID = boosterID;
//         Quantity = quantity;
//     }
// }

#if !UNITY_WEBGL
using Match3;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
[System.Serializable]
public class BoosterSlot
{
    // Serializable fields for Unity
    [SerializeField] private BoosterID boosterId;
    [SerializeField] private int quantity;

    // Firebase properties mapped to serializable fields
    [FirestoreProperty]
    public BoosterID BoosterID 
    { 
        get => boosterId; 
        set => boosterId = value; 
    }

    [FirestoreProperty]
    public int Quantity 
    { 
        get => quantity; 
        set => quantity = value; 
    }

    // Parameterless constructor required by Firestore
    public BoosterSlot() 
    {
      
    }

    // Constructor with parameters
    public BoosterSlot(BoosterID boosterID, int quantity)
    {
        this.boosterId = boosterID;
        this.quantity = quantity;
    }
}
#endif