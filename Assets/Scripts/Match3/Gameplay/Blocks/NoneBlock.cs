using UnityEngine;
using DG.Tweening;
namespace Match3
{
    public class NoneBlock : Block
    {
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            Tile cachedTile = tile;
            grid[tile.X + tile.Y * width] = null;
            Destroy(cachedTile.gameObject);
            return;
            // cachedTile.TileTransform.DOScale(0.8f, 0.2f).SetEase(Ease.InFlash).OnComplete(() =>
            // {
            //     cachedTile.TileTransform.DOScale(1.3f, 0.1f).SetEase(Ease.InFlash).OnComplete(() =>
            //     {
            //         Destroy(cachedTile.gameObject);
            //     });
            // });
            QuestID questID = GameplayManager.Instance.GetQuestByTileID(tile.ID);
            if (GameplayManager.Instance.TryGetQuestIndex(questID, out int index))
            {
                Destroy(cachedTile.gameObject);
            }
            else
            {
                // GameObject psPrefab = Resources.Load<ParticleSystem>("Effects/DestroyBlueTile").gameObject;
                GameObject psPrefab = Resources.Load<ParticleSystem>("Effects/Confetti_blast_multicolor").gameObject;
                if (psPrefab == null)
                {
                    Debug.LogError("Missing prefab");
                    return;
                }
                tile.TileTransform.DOScale(0.9f, 0.05f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    tile.TileTransform.DOScale(1.2f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        ParticleSystem ps = Instantiate(psPrefab, tile.TileTransform.position, Quaternion.identity).GetComponent<ParticleSystem>();
                        ps.Play();
                        Destroy(ps.gameObject, 2f);

                        Destroy(cachedTile.gameObject);

                    });
                });

            }

        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
