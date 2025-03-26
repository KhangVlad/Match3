using System;

namespace Match3
{
    [System.Serializable]
    public class FreeSwitchBooster : Booster
    {
        public FreeSwitchBooster(int quantity) : base(BoosterID.FreeSwitch, quantity)
        {
            Match3Grid.OnEndOfTurn += OnEndOfTurn_TurnOnReswapIfNotMatch;
        }

        ~FreeSwitchBooster()
        {
            Match3Grid.OnEndOfTurn -= OnEndOfTurn_TurnOnReswapIfNotMatch;
        }

        public override void Use()
        {
            base.Use();
            Match3Grid.Instance.HandleReswapIfNotMatch = false;
        }

        private void OnEndOfTurn_TurnOnReswapIfNotMatch()
        {
            Match3Grid.Instance.HandleReswapIfNotMatch = true;
        }
    }
}
