using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class PurpleRibbon : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.PurpleRibbon;
        }

        protected override void Awake()
        {
            base.Awake();
        }


        public override void PlayMatchVFX( MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.PurpleRibbonDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}
