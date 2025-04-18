namespace Match3
{
    public class MagnifyingGlass : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.MagnifyingGlass;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
