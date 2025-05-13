using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class BlueBallon : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.BlueBallon;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void PlayMatchVFX(MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.BlueCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}