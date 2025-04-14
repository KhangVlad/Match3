using UnityEngine;
using DG.Tweening;
using RoboRyanTron.SearchableEnum;

namespace Match3
{
    public abstract class Tile : MonoBehaviour
    {
        public static event System.Action<Tile> OnMatched;

        protected SpriteRenderer sr;
        [field: SerializeField, SearchableEnum] public TileID ID { get; protected set; }
        [SerializeField] protected Sprite _tileSprite;
        [SerializeField] protected Sprite _match4Vertical;
        [SerializeField] protected Sprite _match4Horizontal;
        [SerializeField] protected Sprite _match5;
        [SerializeField] protected Sprite _match6;
        public Transform TilePivot;
        public Transform TileTransform;


        [Header("~Runtime")]
        public Block CurrentBlock;
        public SpecialTileID SpecialProperties;
        public int X;
        public int Y;

        private Tween _moveTween;
        private Tween _scaleTween;


        #region Properties
        public Sprite TileSprite => _tileSprite;
        #endregion

        protected virtual void Awake()
        {
            sr = TileTransform.GetComponent<SpriteRenderer>();
        }

        private void OnDestroy()
        {
            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            if (_scaleTween != null && _scaleTween.IsActive())
            {
                _scaleTween.Kill();
            }
        }

        public void Display(bool enable)
        {
            if (enable)
            {
                sr.color = Color.white;
            }
            else
            {
                sr.color = new Color(255, 255, 255, 0);
            }
        }

        public void SetTileOffset(Vector2 offset)
        {
            TileTransform.localPosition = offset;
        }

        public void ChangeBlock(BlockID blockID)
        {
            Destroy(CurrentBlock.gameObject);

            Block blockPrefab = GameDataManager.Instance.GetBlockByID(blockID);
            Block blockInstance = Instantiate(blockPrefab, this.transform);
            blockInstance.transform.localPosition = Vector3.zero;
            SetBlock(blockInstance);
        }

        public virtual void SetBlock(Block block)
        {
            this.CurrentBlock = block;
            block.transform.localPosition += new Vector3(0.5f, 0.5f, 0f);
            switch (CurrentBlock.BlockID)
            {
                case BlockID.Fill:
                    sr.enabled = false;
                    //sr.enabled = true;
                    block.GetComponent<SpriteRenderer>().enabled = false;
                    sr.color = Color.black;
                    break;
                case BlockID.Void:
                    SetRenderOrder(5);
                    sr.enabled = false;
                    block.GetComponent<SpriteRenderer>().enabled = false;
                    break;
                case BlockID.None:
                    SetRenderOrder(0);
                    sr.enabled = true;
                    break;
                case BlockID.Lock:
                case BlockID.Ice:
                case BlockID.HardIce:
                case BlockID.EternalIce:
                case BlockID.BlackMud:
                case BlockID.Leaf_01:
                case BlockID.Leaf_02:
                case BlockID.Leaf_03:
                case BlockID.Wall_01:
                case BlockID.Wall_02:
                case BlockID.Wall_03:
                    SetRenderOrder(1);
                    sr.enabled = true;
                    break;
                default:
                    sr.enabled = true;
                    Debug.Log($"Case not found: {CurrentBlock.BlockID}");
                    break;
            }
        }

        public virtual void SetSpecialTile(SpecialTileID properties)
        {
            if (this.CurrentBlock is not NoneBlock) return;

            this.SpecialProperties = properties;
            switch (SpecialProperties)
            {
                case SpecialTileID.None:
                    sr.sprite = _tileSprite;
                    break;
                case SpecialTileID.ColumnBomb:
                    sr.sprite = _match4Vertical;
                    break;
                case SpecialTileID.RowBomb:
                    sr.sprite = _match4Horizontal;
                    break;
                case SpecialTileID.BlastBomb:
                    sr.sprite = _match5;
                    break;
                case SpecialTileID.ColorBurst:
                    sr.sprite = _match6;
                    break;
                default:
                    Debug.Log("Case not found");
                    break;
            }
        }

        public virtual void Match(Tile[] grid, int width)
        {
            CurrentBlock.Match(this, grid, width);
            OnMatched?.Invoke(this);
        }

        public virtual void Unlock()
        {
            CurrentBlock.Unlock(this);
        }

        public void MoveToGridPosition(float moveTime = AnimationExtensions.TILE_MOVE_TIME)
        {
            Vector3 targetPosition = this.GetWorldPosition();
            _moveTween = transform.DOMove(targetPosition, moveTime).SetEase(Ease.Linear);
        }
        public void FallDownToGridPosition(float moveTime = AnimationExtensions.TILE_FALLDOWN_TIME)
        {
            Vector3 targetPosition = this.GetWorldPosition();

            _moveTween = transform.DOMove(targetPosition, moveTime * 0.8f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    TilePivot.transform.DOScaleX(1.2f, 0.2f);
                    TilePivot.transform.DOScaleY(0.8f, 0.2f).OnComplete(() =>
                    {
                        TilePivot.transform.DOScaleX(1.0f, 0.2f);
                        TilePivot.transform.DOScaleY(1.0f, 0.2f);
                    });
                });
        }


        public void SetGridPosition(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public void UpdatePosition(int offsetX = 0, int offsetY = 0)
        {
            transform.position = this.GetWorldPosition(offsetX, offsetY);
        }

        public void SetRenderOrder(int order)
        {
            sr.sortingOrder = order;
        }

        public virtual void PlayMatchAnimation()
        {
            _scaleTween = transform.DOScale(0.1f, 0.2f).SetEase(Ease.Linear);
        }

        public bool IsCorrectPosition(out float distance)
        {
            distance = Vector2.Distance(transform.position, this.GetWorldPosition());
            return distance < 0.05f;
        }

        public virtual bool Equal(Tile tile)
        {
            if (tile == null) return false;
            return tile.X == this.X && tile.Y == this.Y;
        }
    }
}
