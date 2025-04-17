using UnityEngine;

namespace Match3
{
    public class Wall01 : Block
    {
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            // Destroy(tile.CurrentBlock.gameObject);

            // Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.Wall_02);
            // Block blockInstance = Instantiate(blockPrefab, tile.transform);
            // blockInstance.transform.localPosition = Vector3.zero;

            // tile.SetBlock(blockInstance);

            tile.ChangeBlock(BlockID.None);
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
