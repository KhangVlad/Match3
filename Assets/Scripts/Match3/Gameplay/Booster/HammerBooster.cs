namespace Match3
{
    [System.Serializable]
    public class HammerBooster : Booster
    {
        public HammerBooster(int quantity) : base(BoosterID.Hammer, quantity)
        { }

        public override void Use()
        {
            base.Use();
        }
    }
}
