namespace Match3
{
    [System.Serializable]
    public class AxisBombBooster : Booster
    {
        public AxisBombBooster(int quantity) : base(BoosterID.AxisBomb, quantity)
        { }

        public override void Use()
        {
            base.Use();
        }
    }
}
