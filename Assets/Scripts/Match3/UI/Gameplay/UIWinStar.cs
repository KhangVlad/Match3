using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    public class UIWinStar : MonoBehaviour
    {
        [SerializeField] private Image _starImage;
        
        [SerializeField] private Sprite _activeStar;
        [SerializeField] private Sprite _unactiveStar;

        


        private void Awake()
        {
            _starImage.sprite = _unactiveStar;
        }

        public void ActiveStar()
        {
            _starImage.sprite = _activeStar;
        }
    }
}
