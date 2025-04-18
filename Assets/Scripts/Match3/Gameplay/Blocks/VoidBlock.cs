namespace Match3
{
    public class VoidBlock : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.Void;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
