using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class PurpleFlower : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.PurpleFlower;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Match(Tile[] grid, int width, MatchID matchID)
        {
            base.Match(grid, width, matchID);
        }

        public override void PlayMatchVFX( MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.PurpleFlowerDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}
