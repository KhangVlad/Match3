namespace Match3
{
    [System.Serializable]
    public class ExtraMoveBooster : Booster
    {
        public ExtraMoveBooster(int quantity) : base(BoosterID.ExtraMove, quantity)
        { }

        public override void Use()
        {
            base.Use();
            GameplayManager.Instance.AddTurnRemaingCount(3);
        }
    }
}
