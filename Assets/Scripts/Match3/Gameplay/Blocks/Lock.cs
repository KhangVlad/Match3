using UnityEngine;
namespace Match3
{
    public class Lock : Block
    {
        public static event System.Action<Lock> OnLockMatch;

         public override void Initialize()
        {
            BlockID = BlockID.Lock;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            Destroy(tile.gameObject);
            grid[tile.X + tile.Y * width] = null;

            OnLockMatch?.Invoke(this);
        }

        public override void Unlock(Tile tile)
        {
            //Debug.Log("Lock unlock");
            //Destroy(tile.CurrentBlock.gameObject);

            //Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.None);
            //Block blockInstance = Instantiate(blockPrefab, tile.transform);
            //blockInstance.transform.localPosition = Vector3.zero;

            //tile.SetBlock(blockInstance);
        }
    }
}
