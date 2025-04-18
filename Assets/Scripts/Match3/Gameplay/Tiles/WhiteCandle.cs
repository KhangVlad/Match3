using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class WhiteCandle : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.WhiteCandle;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Match(Tile[] grid, int width)
        {
            base.Match(grid, width);
            if (GameplayManager.Instance.HasTileQuest(this, out QuestID questID)) return;
           BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.WhiteCandleDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();

        }
    }
}
