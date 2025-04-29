using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class BlueCandle : Tile
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.BlueCandle;
        }

        public override void Match(Tile[] grid, int width)
        {
            base.Match(grid, width);
        }

        public override void PlayMatchVFX()
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.BlueCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}
