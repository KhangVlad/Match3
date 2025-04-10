using System.Collections;
using UnityEngine;

namespace Match3
{
    public class GameplayGridSlot : MonoBehaviour
    {
        private SpriteRenderer _sr;

        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _matchColor;


        public SpriteRenderer SpriteRenderer => _sr;

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {

        }

        public void SetDefaultColor()
        {
            _sr.color = _defaultColor;
        }

        public void SetMatchColor()
        {
            _sr.color = _matchColor;
        }

        public void PlayMatchEffect(float duration)
        {
            StartCoroutine(MatchCoroutine(duration));
        }

        private IEnumerator MatchCoroutine(float duration)
        {
            SetMatchColor();

            float elapsedTime = 0f;
            while (true)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > duration)
                {
                    break;
                }

                // _sr.color = Color.Lerp(_matchColor, _defaultColor, elapsedTime / duration);
                yield return null;
            }

            SetDefaultColor();
        }
    }
}
