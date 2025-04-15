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
        private Vector3[] _path = new Vector3[3];
        private List<Tile> _animatedTiles;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            Match3TileDict = new();
            _animatedTiles = new();
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
                bool hasTile = false;
                for (int i = 0; i < Match3TileDict[key].Count; i++)
                {
                    if (Match3TileDict[key][i].Position == value.Position)
                    {
                        hasTile = true;
                        break;
                    }
                }
                if (hasTile == false)
                    Match3TileDict[key].Add(value);

                //Match3TileDict[key].Add(value);
            }
        }

        public void PlayCollectAnimation()
        {
            StartCoroutine(PlayCollectAnimCoroutine());
        }

        private IEnumerator PlayCollectAnimCoroutine()
        {
            // yield break;
            // foreach(var e in Match3TileDict)
            // {
            //     e.Value.Add(e.Key);
            // }
            
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
                        tileInstance.SetInteractionMask(SpriteMaskInteraction.None);
                        _animatedTiles.Add(tileInstance);
                    }
                }
            }

            Match3TileDict.Clear();

            //Debug.Log($"{Match3TileDict.Count}    {_animatedTiles.Count}");
            for (int i = 0; i < _animatedTiles.Count; i++)
            {
                Tile tile = _animatedTiles[i];
                QuestID questID = GameplayManager.Instance.GetQuestByTileID(tile.ID);
                if (GameplayManager.Instance.TryGetQuestIndex(questID, out int questIndex))
                {
                    Vector2 ssPosition = UIQuestManager.Instance.GetUIQuestSSPosition(questIndex) - new Vector2(0.5f, 0.5f);

                    Vector3 startPos =tile.transform.position;
                    Vector3 endPos = ssPosition;
                    // Control point: this will determine the curve arch
                    Vector3 controlPoint = startPos + new Vector3((endPos.x - startPos.x) * 0.5f, -1f + i * 0.5f, (endPos.z - startPos.z) * 0.5f); // goes downward first
                    _path[0] = startPos;
                    _path[1] = controlPoint;
                    _path[2] = endPos;
                    tile.transform.DOPath(_path, 1.0f, PathType.CatmullRom)
                        .SetEase(Ease.InOutQuad)
                        .OnComplete(() =>
                        {
                            Destroy(tile.gameObject);
                        });

                    yield return new WaitForSeconds(0.05f);
                }
            }
            _animatedTiles.Clear();
            yield return null;
        }


        public void Clear()
        {
            Match3TileDict.Clear();
            _animatedTiles.Clear();
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

        public override bool Equals(object obj)
        {
            if (obj is TilePositionInfo other)
            {
                return ID.Equals(other.ID) && Position.Equals(other.Position);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode() ^ Position.GetHashCode();
        }

        public static bool operator ==(TilePositionInfo a, TilePositionInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TilePositionInfo a, TilePositionInfo b)
        {
            return !a.Equals(b);
        }
    }
}

