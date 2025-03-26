using UnityEngine;

namespace Match3
{
    [System.Serializable]
    public class Booster : IBooster
    {
        public event System.Action OnBoosterUsed;

        public BoosterID BoosterID;
        public int Quantity;
      
        public Booster() { }
        public Booster(BoosterID id, int quantity)
        {
            this.BoosterID = id;
            this.Quantity = quantity;
        }

        public virtual void Use() 
        {
            if (Quantity <= 0) return;
            Quantity--;
            OnBoosterUsed?.Invoke();
        }
    }

    public enum BoosterID : byte 
    {
        ColorBurst = 1,
        BlastBomb = 2,
        AxisBomb = 3,
        ExtraMove = 4,
        FreeSwitch = 5,
        Hammer = 6,
    }
}
