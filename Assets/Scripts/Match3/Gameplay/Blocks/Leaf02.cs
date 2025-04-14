using UnityEngine;

namespace Match3
{   
    public class Leaf02 : Block
    {
        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
             Destroy(tile.CurrentBlock.gameObject);

            Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.Leaf_01);
            Block blockInstance = Instantiate(blockPrefab, tile.transform);
            blockInstance.transform.localPosition = Vector3.zero;

            tile.SetBlock(blockInstance);
        }
    }
}
