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
        public Dictionary<TilePositionInfo, List<TilePositionInfo>> AnotherMatchTileDict;
        private HashSet<TilePositionInfo> AnotherMatchSet;
        private Vector3[] _path = new Vector3[3];
        private List<Tile> _animatedTiles;
        private float TILE_MOVE_PATH_TIME = 0.8f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            Match3TileDict = new();
            AnotherMatchSet = new();
            AnotherMatchTileDict = new();
            _animatedTiles = new();
        }

        public void AddMatch3(TilePositionInfo key, TilePositionInfo value)
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
                    if (Match3TileDict[key][i].Index == value.Index)
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

        public void AddAnotherMatch(TilePositionInfo key, TilePositionInfo value)
        {
            if (AnotherMatchTileDict.ContainsKey(key) == false)
            {
                List<TilePositionInfo> valueData = new();
                AnotherMatchTileDict.Add(key, valueData);
                AnotherMatchTileDict[key].Add(value);
            }
            else
            {
                bool hasTile = false;
                for (int i = 0; i < AnotherMatchTileDict[key].Count; i++)
                {
                    if (AnotherMatchTileDict[key][i].Index == value.Index)
                    {
                        hasTile = true;
                        break;
                    }
                }
                if (hasTile == false)
                    AnotherMatchTileDict[key].Add(value);
            }
        }

        public void PlayCollectAnimation()
        {
            StartCoroutine(PlayCollectAnimationCoroutine());
        }

        public IEnumerator PlayCollectAnimationCoroutine()
        {
            yield return StartCoroutine(PlayCollectMatch3AnimCoroutine());
            yield return StartCoroutine(PlayCollectAnotherMatchAnimCoroutine());
        }



        private IEnumerator PlayCollectMatch3AnimCoroutine()
        {
            if (Match3TileDict.Count == 0) yield break;

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

            _animatedTiles.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            //Debug.Log($"{Match3TileDict.Count}    {_animatedTiles.Count}");
            for (int i = 0; i < _animatedTiles.Count; i++)
            {
                Tile tile = _animatedTiles[i];
                QuestID questID = GameplayManager.Instance.GetQuestByTileID(tile.ID);
                if (GameplayManager.Instance.TryGetQuestIndex(questID, out int questIndex))
                {
                    Vector2 ssPosition = UIQuestManager.Instance.GetUIQuestSSPosition(questIndex) - new Vector2(0.5f, 0.5f);

                    Vector3 startPos = tile.transform.position;
                    Vector3 endPos = ssPosition;
                    // Control point: this will determine the curve arch
                    Vector3 controlPoint = startPos + new Vector3((endPos.x - startPos.x) * 0.5f, -1f + i * 0.5f, (endPos.z - startPos.z) * 0.5f); // goes downward first
                    _path[0] = startPos;
                    _path[1] = controlPoint;
                    _path[2] = endPos;
                    Tween moveTween = tile.transform.DOPath(_path, TILE_MOVE_PATH_TIME, PathType.CatmullRom)
                        .SetEase(Ease.InOutQuad)
                        .OnComplete(() =>
                        {
                            Destroy(tile.gameObject);
                        });
                    yield return new WaitForSeconds(0.05f);
                }
            }
            _animatedTiles.Clear();
            Match3TileDict.Clear();
            yield return null;
        }

        private IEnumerator PlayCollectAnotherMatchAnimCoroutine()
        {
            if (AnotherMatchTileDict.Count == 0) yield break;

            foreach (var e in AnotherMatchTileDict)
            {
                for (int i = 0; i < e.Value.Count; i++)
                {
                    TilePositionInfo tileInfo = e.Value[i];
                    QuestID questID = GameplayManager.Instance.GetQuestByTileID(tileInfo.ID);
                    if (GameplayManager.Instance.TryGetQuestIndex(questID, out int questIndex))
                    {
                        if (AnotherMatchSet.Contains(tileInfo) == false)
                        {
                            AnotherMatchSet.Add(tileInfo);
                        }
                    }
                }
            }

            foreach (var tileInfo in AnotherMatchSet)
            {
                Tile tilePrefab = GameDataManager.Instance.GetTileByID(tileInfo.ID);
                Tile tileInstance = Instantiate(tilePrefab, tileInfo.Position, Quaternion.identity);
                // tileInstance.transform.localScale = new Vector3(0.2f,0.2f,1f);
                tileInstance.StopEmissive(5, 0, 1f);
                tileInstance.SetRenderOrder(100);
                tileInstance.SetInteractionMask(SpriteMaskInteraction.None);
                _animatedTiles.Add(tileInstance);
            }


            for (int i = 0; i < _animatedTiles.Count; i++)
            {
                Tile tile = _animatedTiles[i];
                QuestID questID = GameplayManager.Instance.GetQuestByTileID(tile.ID);
                if (GameplayManager.Instance.TryGetQuestIndex(questID, out int questIndex))
                {
                    Vector2 ssPosition = UIQuestManager.Instance.GetUIQuestSSPosition(questIndex) - new Vector2(0.5f, 0.5f);
                    Vector3 startPos = tile.transform.position;
                    Vector3 endPos = ssPosition;
                    // Control point: this will determine the curve arch
                    Vector3 controlPoint = startPos + new Vector3((endPos.x - startPos.x) * 0.8f, -1f + (i * 1.5f) * 0.5f, (endPos.z - startPos.z) * 0.5f); // goes downward first
                    _path[0] = startPos;
                    _path[1] = controlPoint;
                    _path[2] = endPos;
                    tile.transform.DOPath(_path, TILE_MOVE_PATH_TIME, PathType.CatmullRom)
                         .SetEase(Ease.InOutQuad)
                         .OnComplete(() =>
                         {
                             Destroy(tile.gameObject);
                         });
                    tile.transform.DOScale(0.7f, TILE_MOVE_PATH_TIME).SetEase(Ease.InBack);
                    yield return new WaitForSeconds(0.1f);
                }
            }

            AnotherMatchTileDict.Clear();
            AnotherMatchSet.Clear();
            _animatedTiles.Clear();
            yield return null;
        }


        // public void Clear()
        // {
        //     Match3TileDict.Clear();
        //     Match4TileDict.Clear();
        //     _animatedTiles.Clear();
        // }
    }


    [System.Serializable]
    public struct TilePositionInfo
    {
        public TileID ID;
        public Vector2 Position;
        public int Index;

        public TilePositionInfo(TileID id, Vector2 position, int index)
        {
            this.ID = id;
            this.Position = position;
            this.Index = index;
        }

        public override bool Equals(object obj)
        {
            if (obj is TilePositionInfo other)
            {
                return ID.Equals(other.ID) && Position.Equals(other.Position) && Index.Equals(other.Index);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode() ^ Position.GetHashCode() ^ Index.GetHashCode();
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

