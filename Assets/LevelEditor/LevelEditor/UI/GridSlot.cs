using UnityEngine;

namespace Match3.LevelEditor
{
    public class GridSlot : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sr;

        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _hoverColor;

        public void Hover(bool hover)
        {
            if(hover)
            {
                _sr.color = _hoverColor;
            }
            else
            {
                _sr.color= _defaultColor;
            }
        }
    }
}
