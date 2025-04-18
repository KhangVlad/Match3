using UnityEngine;

namespace Match3
{
    public class Ice : Block
    {
        public static event System.Action<Ice> OnIceUnlocked;

        public override void Initialize()
        {
            BlockID = BlockID.Ice;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
            tile.ChangeBlock(BlockID.None);
            OnIceUnlocked?.Invoke(this);
        }
    }
}
