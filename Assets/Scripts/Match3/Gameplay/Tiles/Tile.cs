using UnityEngine;
using DG.Tweening;
using RoboRyanTron.SearchableEnum;
using FMOD.Studio;
using System.Collections;
using UnityEngine.Pool;

namespace Match3
{
    [SelectionBase]
    public abstract class Tile : MonoBehaviour
    {
        public static event System.Action<Tile> OnMatched;
        protected ObjectPool<Tile> pool;

        protected SpriteRenderer sr;
        protected SpriteRenderer bloomSR;
        public TileID ID { get; protected set; }
        protected Sprite _tileSprite;
        [SerializeField] protected Sprite _match4Vertical;
        [SerializeField] protected Sprite _match4Horizontal;
        [SerializeField] protected Sprite _match5;
        [SerializeField] protected Sprite _match6;
        public Transform TilePivot;
        public Transform TileTransform;


        // mask interaction
        private MaterialPropertyBlock _propBlock;
        private Coroutine _emissiveCoroutine;


        [Header("~Runtime")]
        public Block CurrentBlock;
        public SpecialTileID SpecialProperties;
        public int X;
        public int Y;

        private Tween _moveTween;
        private Tween _scaleTween;
        private Tween _tileScaleTween;
        private Tween _shakeTween;


        #region Properties
        public SpriteRenderer TileSR => sr;
        public Sprite TileSprite => _tileSprite;
        public bool IsDisplay { get; private set; } = true;
        public bool HasTriggeredSpecial { get; private set; } = false;
        #endregion


        public virtual void Initialize()
        {
            if (TileTransform == null)
            {
                TileTransform = transform.Find("Pivot/Tile");
            }

            bloomSR = TileTransform.GetChild(0).GetComponent<SpriteRenderer>();
            bloomSR.enabled = false;
            if (bloomSR == null) Debug.LogError("Missing bloom reference !!!");
            Bloom(false);
            sr = TileTransform.GetComponent<SpriteRenderer>();
            _tileSprite = sr.sprite;
            _propBlock = new MaterialPropertyBlock();
        }

        protected virtual void Awake()
        {
            if (sr == null)
            {
                Initialize();
            }
            Bloom(false);
        }


        private void OnDestroy()
        {
            ClearAllTweens();
        }

        protected void OnEnable()
        {

        }

        protected void Oisable()
        {
            Display(true);
        }

        private void ClearAllTweens()
        {
            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            if (_scaleTween != null && _scaleTween.IsActive())
            {
                _scaleTween.Kill();
            }

            if (_tileScaleTween != null && _tileScaleTween.IsActive())
            {
                _tileScaleTween.Kill();
            }

            if (_shakeTween != null && _shakeTween.IsActive())
            {
                _shakeTween.Kill();
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
            IsDisplay = enable;
        }

        public void PlayAppearAnimation(float duration)
        {
            TileTransform.localScale = Vector3.zero;
            _scaleTween = TileTransform.DOScale(1f, duration).SetEase(Ease.OutBack);
        }

        public void SetTileOffset(Vector2 offset)
        {
            TileTransform.localPosition = offset;
        }


        public void ChangeBlock(BlockID blockID)
        {
            if (CurrentBlock != null)
            {
                CurrentBlock.ReturnToPool();
                CurrentBlock = null;
            }
            // Destroy(CurrentBlock?.gameObject);

            // Block blockPrefab = GameDataManager.Instance.GetBlockByID(blockID);
            // Block blockInstance = Instantiate(blockPrefab, this.transform);
            // blockInstance.transform.localPosition = Vector3.zero;
            Block blockInstance = BlockPoolManager.Instance.GetBlock(blockID);
            blockInstance.transform.SetParent(this.transform);

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
                case BlockID.Bush_01:
                case BlockID.Bush_02:
                case BlockID.Bush_03:
                case BlockID.Wall_01:
                case BlockID.Wall_02:
                case BlockID.Wall_03:
                case BlockID.Spider:
                case BlockID.SpiderNet:
                case BlockID.SpiderOnNet:
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
            if (_emissiveCoroutine != null)
            {
                StopCoroutine(_emissiveCoroutine);
            }
            if (GameplayManager.Instance.HasTileQuest(this, out QuestID questID) == false)
            {
                if (IsDisplay)
                {
                    PlayMatchVFX();
                }
            }

            CurrentBlock.Match(this, grid, width);
            OnMatched?.Invoke(this);
        }

        public virtual void Unlock()
        {
            CurrentBlock.Unlock(this);
        }

        public virtual void PlayMatchVFX() { }

        public void MoveToPosition(Vector2 targetPosition, float moveTime, Ease ease)
        {
            _moveTween = transform.DOMove(targetPosition, moveTime).SetEase(ease);
        }

        public void MoveToGridPosition(float moveTime = TileAnimationExtensions.TILE_MOVE_TIME)
        {
            Vector3 targetPosition = this.GetWorldPosition();
            _moveTween = transform.DOMove(targetPosition, moveTime).SetEase(Ease.Linear);
        }
        public void FallDownToGridPosition(float moveTime = TileAnimationExtensions.TILE_FALLDOWN_TIME)
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

        public void SetInteractionMask(SpriteMaskInteraction mask)
        {
            sr.maskInteraction = mask;
        }

        public void Bloom(bool enable)
        {
            bloomSR.enabled = enable;
        }


        public void Emissive(float duration)
        {
            if (_emissiveCoroutine != null)
                return;
            _emissiveCoroutine = StartCoroutine(EmissiveCoroutine(0f, 5f, duration));
        }

        private IEnumerator EmissiveCoroutine(float startValue, float endValue, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentValue = Mathf.Lerp(startValue, endValue, elapsed / duration);
                sr.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat("_EmissionStrength", currentValue);
                sr.SetPropertyBlock(_propBlock);
                yield return null;
            }

            // Ensure it ends at the final value
            sr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_EmissionStrength", endValue);
            sr.SetPropertyBlock(_propBlock);
        }



        public void StopEmissive(float startValue = 5f, float endValue = 0f, float duration = 0.2f)
        {
            if (_emissiveCoroutine != null)
            {
                StopCoroutine(_emissiveCoroutine);
            }
            _emissiveCoroutine = StartCoroutine(EmissiveCoroutine(startValue, endValue, duration));
        }


        public virtual void SetTriggerSpecial(bool triggered)
        {
            HasTriggeredSpecial = triggered;
        }

        public virtual void PlayShaking(float duration)
        {
            _shakeTween = transform.DOShakePosition(
                duration: duration,     // Longer duration = slower overall shake
                strength: 0.1f,         // Smaller strength = smaller movement
                vibrato: 5,             // Fewer shakes = slower, less intense
                randomness: 3f         // Less randomness = more controlled movement
            );
        }
        public virtual void PlayScaleTile(float endValue, float duration, Ease ease)
        {
            _tileScaleTween = TileTransform.DOScale(endValue, duration).SetEase(ease);
        }
        public virtual void MovePath(Vector3[] path, float duration, PathType pathType, Ease ease, System.Action oncompleted)
        {
            _moveTween = transform.DOPath(path, duration, PathType.CatmullRom)
                       .SetEase(Ease.InOutQuad)
                       .OnComplete(() =>
                       {
                           oncompleted?.Invoke();
                       });
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


        #region  Pool
        public void SetPool(ObjectPool<Tile> pool)
        {
            this.pool = pool;
        }

        public virtual void ReturnToPool()
        {
            pool?.Release(this);
            ResetTile();
        }

        public virtual void ReturnToPool(float duration)
        {
            Invoke(nameof(ReturnToPool), duration);
        }

        private void ResetTile()
        {
            // X = 0;
            // Y = 0;
            transform.localScale = Vector3.one;
            TileTransform.localScale = Vector3.one;
            TilePivot.localScale = Vector3.one;

            ClearAllTweens();
            ChangeBlock(BlockID.None);
            SpecialProperties = SpecialTileID.None;

            sr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_EmissionStrength", 0);
            sr.SetPropertyBlock(_propBlock);
            Bloom(false);
            SetTriggerSpecial(false);
        }
        #endregion
    }
}
