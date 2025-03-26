using UnityEngine;

namespace Match3
{
    public interface IBlock
    {
        public bool IsUnlock { get; set; }
        public void Unlock(Tile tile);
        public void Match(Tile tile, Tile[] grid, int width);
    }
}
