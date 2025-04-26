using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class GreenCandle : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.GreenCandle;
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
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.GreenCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }
}
