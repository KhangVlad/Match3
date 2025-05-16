using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Fire : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Fire;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}