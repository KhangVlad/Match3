using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class RedChilie : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Red_Chillie;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}