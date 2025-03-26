using UnityEngine;
using RoboRyanTron.SearchableEnum;
namespace Match3
{
    public abstract class Block : MonoBehaviour, IBlock
    {
        [field: SerializeField, SearchableEnum] public BlockID BlockID { get; set; }
        public bool IsUnlock { get; set; } = false;

        public abstract void Match(Tile tile, Tile[] grid, int width);
        public abstract void Unlock(Tile tile);
    }

}
