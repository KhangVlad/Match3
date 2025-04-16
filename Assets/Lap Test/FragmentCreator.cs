using UnityEngine;
using System.Collections.Generic;

namespace LapTest
{
    public class FragmentCreator : MonoBehaviour
    {
        [SerializeField] private Texture2D _source;
        [SerializeField] private List<Texture2D> _pieces;
        private void Start()
        {
            _pieces = SliceTexture(_source, 2, 4);
        }

        public List<Texture2D> SliceTexture(Texture2D source, int columns, int rows)
        {
            List<Texture2D> fragments = new List<Texture2D>();
            int width = source.width / columns;
            int height = source.height / rows;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Texture2D fragment = new Texture2D(width, height);
                    fragment.SetPixels(source.GetPixels(x * width, y * height, width, height));
                    fragment.Apply();
                    fragments.Add(fragment);
                }
            }

            return fragments;
        }
    }
}
