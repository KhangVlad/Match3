using Match3.Shares;
using Match3.Enums;
namespace Match3
{
     public class YellowBut : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.YellowBut;
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