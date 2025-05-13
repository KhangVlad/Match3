using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class WhiteFlower : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.WhiteFlower;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    
        public override void PlayMatchVFX( MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.WhiteCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}
