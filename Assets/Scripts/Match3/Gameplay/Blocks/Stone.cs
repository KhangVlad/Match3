using UnityEngine;
namespace Match3
{
    public class Stone : Block
    {
        public static event System.Action<Stone> OnStoneMatch;
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            //Destroy(tile.CurrentBlock.gameObject);

            //Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.None);
            //Block blockInstance = Instantiate(blockPrefab, tile.transform);
            //blockInstance.transform.localPosition = Vector3.zero;


            //tile.SetBlock(blockInstance);

            Destroy(tile.gameObject);
            grid[tile.X + tile.Y * width] = null;

            OnStoneMatch?.Invoke(this);
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
