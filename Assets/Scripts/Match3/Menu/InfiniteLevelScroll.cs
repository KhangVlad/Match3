using UnityEngine;
using DG.Tweening;

namespace Match3
{
    public class InfiniteLevelScroll : MonoBehaviour
    {
        [SerializeField] private RectTransform _background01;
        [SerializeField] private RectTransform _background02;
        [SerializeField] private RectTransform _background03;

        private const float BG_WIDTH = 1080;
        private const float BG_HEIGHT = 1920;

        private void Awake()
        {
            // Debug.Log($"A : {_background01.anchoredPosition}");
            // Debug.Log($"B : {_background02.anchoredPosition}");
            // Debug.Log($"C : {_background03.anchoredPosition}");
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.UpArrow))
        //    {
        //        _background01.DOAnchorPosY(_background01.anchoredPosition.y - 500, 0.2f);
        //        _background02.DOAnchorPosY(_background02.anchoredPosition.y - 500, 0.2f);
        //        _background03.DOAnchorPosY(_background03.anchoredPosition.y - 500, 0.2f).OnComplete(() =>
        //        {
        //            UpdateAllBackgroundOrderMoveUp();
        //        });
        //    }

        //    if (Input.GetKeyDown(KeyCode.DownArrow))
        //    {
        //        _background01.DOAnchorPosY(_background01.anchoredPosition.y + 500, 0.2f);
        //        _background02.DOAnchorPosY(_background02.anchoredPosition.y + 500, 0.2f);
        //        _background03.DOAnchorPosY(_background03.anchoredPosition.y + 500, 0.2f).OnComplete(() =>
        //        {
        //            UpdateAllBackgroundOrderMoveDown();
        //        });
        //    }
        //}

        private void UpdateAllBackgroundOrderMoveDown()
        {
            if (_background01.anchoredPosition.y > 0 && _background01.anchoredPosition.y < BG_HEIGHT)
            {
                _background03.anchoredPosition = new Vector2(_background03.anchoredPosition.x, _background01.anchoredPosition.y - BG_HEIGHT);
            }
            else if (_background01.anchoredPosition.y > BG_HEIGHT * 2)
            {
                _background01.anchoredPosition = new Vector2(_background01.anchoredPosition.x, _background02.anchoredPosition.y - BG_HEIGHT);
            }
            else if (_background01.anchoredPosition.y > BG_HEIGHT)
            {
                _background02.anchoredPosition = new Vector2(_background02.anchoredPosition.x, _background03.anchoredPosition.y - BG_HEIGHT);
            }

            // if (_background01.anchoredPosition.y > 0 && _background01.anchoredPosition.y < BG_HEIGHT)
            // {
            //     _background03.anchoredPosition = new Vector2(_background03.anchoredPosition.x, _background01.anchoredPosition.y - BG_HEIGHT);
            // }
            // else if (_background01.anchoredPosition.y > BG_HEIGHT * 2)
            // {
            //     _background01.anchoredPosition = new Vector2(_background01.anchoredPosition.x, _background02.anchoredPosition.y - BG_HEIGHT);
            // }
            // else if (_background01.anchoredPosition.y > BG_HEIGHT)
            // {
            //     _background02.anchoredPosition = new Vector2(_background02.anchoredPosition.x, _background03.anchoredPosition.y - BG_HEIGHT);
            // }



        }

        private void UpdateAllBackgroundOrderMoveUp()
        {
            if (_background01.anchoredPosition.y < BG_HEIGHT && _background01.anchoredPosition.y > 0)
            {
                _background02.anchoredPosition = new Vector2(_background02.anchoredPosition.x, _background01.anchoredPosition.y + BG_HEIGHT);
            }
            else if (_background01.anchoredPosition.y > -BG_HEIGHT)
            {
                _background03.anchoredPosition = new Vector2(_background03.anchoredPosition.x, _background02.anchoredPosition.y + BG_HEIGHT);
            }
            else if (_background01.anchoredPosition.y > -BG_HEIGHT * 2)
            {
                _background01.anchoredPosition = new Vector2(_background01.anchoredPosition.x, _background03.anchoredPosition.y + BG_HEIGHT);
            }


            // float currentY = _background01.anchoredPosition.y;

            // float bgHeight = BG_HEIGHT;

            // if (currentY < bgHeight && currentY > 0)
            // {
            //     // Section 1: Between 0 and BG height, move background02 above background01
            //     _background02.anchoredPosition = new Vector2(_background02.anchoredPosition.x, currentY + bgHeight);
            // }
            // else if (currentY > -bgHeight)
            // {
            //     // Section 0: Between -BG height and 0, move background03 above background02
            //     _background03.anchoredPosition = new Vector2(_background03.anchoredPosition.x, _background02.anchoredPosition.y + bgHeight);
            // }
            // else if (currentY > -bgHeight * 2)
            // {
            //     // Section -1: Between -2*BG height and -BG height, move background01 above background03
            //     _background01.anchoredPosition = new Vector2(_background01.anchoredPosition.x, _background03.anchoredPosition.y + bgHeight);
            // }
        }
    }
}
