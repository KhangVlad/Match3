using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Soap : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Soap;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}