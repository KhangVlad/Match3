namespace Match3
{
    [System.Serializable]
    public class ColorBurstBooster : Booster
    {
        public ColorBurstBooster(int quantity) : base(BoosterID.ColorBurst, quantity)
        {  }

        public override void Use()
        {
            base.Use();
        }
    }
}
