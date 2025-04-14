using UnityEngine;

namespace Match3
{
    public class Wall03 : Block
    {
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            // Destroy(tile.gameObject);
            // grid[tile.X + tile.Y * width] = null;

            Destroy(tile.CurrentBlock.gameObject);

            Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.None);
            Block blockInstance = Instantiate(blockPrefab, tile.transform);
            blockInstance.transform.localPosition = Vector3.zero;

            tile.SetBlock(blockInstance);

        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
