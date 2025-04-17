using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class RedFlower : Tile
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public override void Match(Tile[] grid, int width)
        {
            base.Match(grid, width);

            if (GameplayManager.Instance.HasTileQuest(this, out QuestID questID))  return;

            if (GameDataManager.Instance.TryGetVfxByID(VisualEffectID.RedFlowerDestroy, out BaseVisualEffect redFlowerVfx))
            {
                RedFlowerVfx redFlowerInstance = Instantiate((RedFlowerVfx)redFlowerVfx, TileTransform.position, Quaternion.identity);
                Destroy(redFlowerInstance.gameObject, 1f);
            }
        }
    }
}
