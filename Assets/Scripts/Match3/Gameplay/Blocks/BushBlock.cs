namespace Match3
{
    public class BushBlock : Block
    {
        public static event System.Action<BushBlock> OnBushMatched;
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            Destroy(tile.gameObject);
            grid[tile.X + tile.Y * width] = null;

            OnBushMatched?.Invoke(this);
        }

        public override void Unlock(Tile tile)
        {
            
        }
    }
}
