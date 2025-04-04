using Match3;

[System.Serializable]
public class BoosterSlot
{
    public BoosterID BoosterID;
    public int Quantity;

    public BoosterSlot(BoosterID boosterID, int quantity)
    {
        BoosterID = boosterID;
        Quantity = quantity;
    }
}