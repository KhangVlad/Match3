#if !UNITY_WEBGL
using Match3;
using Firebase.Firestore;

[FirestoreData]
[System.Serializable]
public class BoosterSlot
{
    [FirestoreProperty]
    public BoosterID BoosterID { get; set; }

    [FirestoreProperty]
    public int Quantity { get; set; }

    // Parameterless constructor required by Firestore
    public BoosterSlot() {}

    public BoosterSlot(BoosterID boosterID, int quantity)
    {
        BoosterID = boosterID;
        Quantity = quantity;
    }
}
#endif