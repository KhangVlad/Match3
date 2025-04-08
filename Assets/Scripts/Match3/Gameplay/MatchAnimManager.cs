using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

namespace Match3
{
    public class MatchAnimManager : MonoBehaviour
    {
        public static MatchAnimManager Instance { get; private set; }
        public Dictionary<TilePositionInfo, List<TilePositionInfo>> Match3TileDict;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            Match3TileDict = new();
        }

        public void Add(TilePositionInfo key, TilePositionInfo value)
        {
            if (Match3TileDict.ContainsKey(key) == false)
            {
                List<TilePositionInfo> valueData = new();
                Match3TileDict.Add(key, valueData);
                Match3TileDict[key].Add(value);
            }
            else
            {
                Match3TileDict[key].Add(value);
            }
        }

        public void PlayCollectAnimation()
        {
            StartCoroutine(PlayCollectAnimCoroutine());
        }

        private IEnumerator PlayCollectAnimCoroutine()
        {
            foreach (var e in Match3TileDict)
            {
                for (int i = 0; i < e.Value.Count; i++)
                {
                    TilePositionInfo tileInfo = e.Value[i];

                    QuestID questID = GameplayManager.Instance.GetQuestByTileID(tileInfo.ID);
                    if (GameplayManager.Instance.TryGetQuestIndex(questID, out int questIndex))
                    {
                        Tile tilePrefab = GameDataManager.Instance.GetTileByID(tileInfo.ID);
                        Tile tileInstance = Instantiate(tilePrefab, tileInfo.Position, Quaternion.identity);
                        tileInstance.SetRenderOrder(100);
                        Vector2 ssPosition = UIQuestManager.Instance.GetUIQuestSSPosition(questIndex) - new Vector2(0.5f, 0.5f);

                        // tileInstance.transform.DOMove(ssPosition, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                        // {
                        //     Destroy(tileInstance.gameObject);
                        // });


                        // Store the original position
                        Vector3 originalPosition = tileInstance.transform.position;
                        tileInstance.TileTransform.DORotate(new Vector3(0, 0, 720), 2f, RotateMode.FastBeyond360)
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Incremental);
                        tileInstance.TileTransform.DOScale(0.75f, 1.0f).SetEase(Ease.Linear);
                        tileInstance.transform.DOMove(ssPosition + new Vector2(0.0f, 0.5f), 1.0f).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            tileInstance.TileTransform.DOScale(0.0f, 0.5f).SetEase(Ease.InFlash);
                            tileInstance.transform.DOMove(ssPosition, 0.5f).SetEase(Ease.InFlash).OnComplete(() =>
                            {
                                Destroy(tileInstance.gameObject);
                            });
                        });

                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            Clear();
        }


        public void Clear()
        {
            Match3TileDict.Clear();
        }
    }


    [System.Serializable]
    public struct TilePositionInfo
    {
        public TileID ID;
        public Vector2 Position;

        public TilePositionInfo(TileID id, Vector2 position)
        {
            this.ID = id;
            this.Position = position;
        }
    }
}

