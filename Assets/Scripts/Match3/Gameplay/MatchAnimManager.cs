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
                    Tile tilePrefab = GameDataManager.Instance.GetTileByID(tileInfo.ID);
                    Tile tileInstance = Instantiate(tilePrefab, e.Key.Position, Quaternion.identity);
                    tileInstance.SetRenderOrder(100);


                    tileInstance.transform.DOMove(new Vector3(0, 10, 0), 1f).SetEase(Ease.Linear);
                    yield return new WaitForSeconds(0.1f);
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

