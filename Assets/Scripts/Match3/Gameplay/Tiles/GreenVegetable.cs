using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class GreenVegetable : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Green_Vegetable;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}