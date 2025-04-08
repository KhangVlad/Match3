using UnityEngine;
using DG.Tweening;
namespace Match3
{
    public class NoneBlock : Block
    {
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            Tile cachedTile = tile;
            cachedTile.TileTransform.DOScale(0.8f, 0.2f).SetEase(Ease.InFlash).OnComplete(() =>
            {
                cachedTile.TileTransform.DOScale(1.3f, 0.1f).SetEase(Ease.InFlash).OnComplete(() =>
                {
                    Destroy(cachedTile.gameObject);
                });
            });
            grid[tile.X + tile.Y * width] = null;     
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
