using Match3.Shares;
using Match3.Enums;
namespace Match3
{
     public class GreenFeather : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.GreenFeather;
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
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.GreenCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }

}