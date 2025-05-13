using Match3.Shares;
using Match3.Enums;
using System;

namespace Match3
{
    public class Onion : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Onion;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void PlayMatchVFX(MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.YellowCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}