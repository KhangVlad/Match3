namespace Match3
{
    public class NoneTile : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.None;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
