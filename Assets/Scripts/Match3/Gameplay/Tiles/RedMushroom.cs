using Match3.Shares;
using Match3.Enums;
namespace Match3
{
    public class RedMushroom : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.RedMushroom;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void PlayMatchVFX(MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.RedCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}