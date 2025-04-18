namespace Match3
{
    public class FillBlock : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.Fill;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
