using UnityEngine;

namespace Match3
{
    public class Leaf03 : Block
    {
        public int ExistTurnCount { get; set; } = 0;

        private void Awake()
        {
            ExistTurnCount = 0;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
            Destroy(tile.CurrentBlock.gameObject);

            Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.None);
            Block blockInstance = Instantiate(blockPrefab, tile.transform);
            blockInstance.transform.localPosition = Vector3.zero;

            tile.SetBlock(blockInstance);
        }
    }
}
