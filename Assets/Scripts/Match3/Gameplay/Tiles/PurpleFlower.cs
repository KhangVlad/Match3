using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class PurpleFlower : Tile
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public override void Match(Tile[] grid, int width)
        {
            base.Match(grid, width);
            if (GameplayManager.Instance.HasTileQuest(this, out QuestID questID)) return;
            if (GameDataManager.Instance.TryGetVfxByID(VisualEffectID.PurpleFlowerDestroy, out BaseVisualEffect vfxPrefab))
            {
                var vfxInstance = Instantiate(vfxPrefab, TileTransform.position, Quaternion.identity);
                Destroy(vfxInstance.gameObject, 1f);
            }
        }
    }
}
