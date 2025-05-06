using Match3.Shares;
using Match3.Enums;
namespace Match3
{
     public class WhiteBut : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.WhiteBut;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Match(Tile[] grid, int width)
        {
            base.Match(grid, width);
        }

        public override void PlayMatchVFX()
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.WhiteCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}