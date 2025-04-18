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
            Destroy(tile.CurrentBlock.gameObject);

            Block blockPrefab = GameDataManager.Instance.GetBlockByID(BlockID.None);
            Block blockInstance = Instantiate(blockPrefab, tile.transform);
            blockInstance.transform.localPosition = Vector3.zero;

            tile.SetBlock(blockInstance);

            OnIceUnlocked?.Invoke(this);
        }
    }
}
