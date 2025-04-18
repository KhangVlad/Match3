namespace Match3
{
    public class Chest : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.Chest;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
