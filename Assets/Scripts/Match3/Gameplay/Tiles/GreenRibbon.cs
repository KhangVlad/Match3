using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class GreenRibbon : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.GreenRibbon;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void PlayMatchVFX(MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.GreenRibbonDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}
