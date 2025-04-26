using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class RedRibbon : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.RedRibbon;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Match(Tile[] grid, int width)
        {
            base.Match(grid, width);
            if (GameplayManager.Instance.HasTileQuest(this, out QuestID questID)) return;
            if (IsDisplay)
            {
                PlayMatchVFX();
            }
        }

        public override void PlayMatchVFX()
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.RedRibbonDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}
