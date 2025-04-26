using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Match3
{
    public class UIQuest : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _completeMarkObject;

        public Image IconImage => _iconImage;

        private Tween _collectTween;
        private bool _canCollect = true;

        private void Awake()
        {
            // InvokeRepeating(nameof(PlayCollectAnimation), 1f, 0.2f);
        }

        public void SetData(Sprite sprite, int quantity)
        {
            this._iconImage.sprite = sprite;
            this._quantityText.text = quantity.ToString();
        }

        public void UpdateQuest(Quest quest)
        {
            if (quest.QuestID == QuestID.MaxTurn)
            {
                _quantityText.text = quest.Quantity.ToString();
            }
            else
            {
                if (quest.Quantity <= 0)
                {
                    _completeMarkObject.gameObject.SetActive(true);
                    _quantityText.text = "";
                }
                else
                {
                    _completeMarkObject.gameObject.SetActive(false);
                    _quantityText.text = quest.Quantity.ToString();
                }
            }
        }

        public void PlayCollectAnimation()
        {
            if (_canCollect == false) return;
            _canCollect = false;
            _collectTween = transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                transform.DOScale(1.0f, 0.1f).SetEase(Ease.OutBack);
                _canCollect = true;
            });
        }
    }
}
