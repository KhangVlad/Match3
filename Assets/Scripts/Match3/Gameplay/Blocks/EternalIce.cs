using UnityEngine;

namespace Match3
{
    public class EternalIce : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.EternalIce;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            
        }

        public override void Unlock(Tile tile)
        {
            Destroy(tile.CurrentBlock.gameObject);

            Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.HardIce);
            Block blockInstance = Instantiate(blockPrefab, tile.transform);
            blockInstance.transform.localPosition = Vector3.zero;


            tile.SetBlock(blockInstance);
        }
    }
}
