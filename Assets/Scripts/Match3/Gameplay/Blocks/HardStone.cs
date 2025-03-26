using UnityEngine;
namespace Match3
{
    public class HardStone : Block
    {
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            Destroy(tile.CurrentBlock.gameObject);

            Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.Stone);
            Block blockInstance = Instantiate(blockPrefab, tile.transform);
            blockInstance.transform.localPosition = Vector3.zero;


            tile.SetBlock(blockInstance);
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
