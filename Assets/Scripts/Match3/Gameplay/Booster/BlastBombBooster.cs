namespace Match3
{
    [System.Serializable]
    public class BlastBombBooster : Booster
    {
        public BlastBombBooster(int quantity) : base(BoosterID.BlastBomb, quantity)
        { }

        public override void Use()
        {
            base.Use();
        }
    }
}
