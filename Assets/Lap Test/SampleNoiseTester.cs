using UnityEngine;

namespace LapTest
{
    public class SampleNoiseTester : MonoBehaviour
    {
        [SerializeField] private Texture2D _noiseTexture;
        [SerializeField] private Texture2D _outputTexture;

        private void Start()
        {
            ReadNoiseTexture(_noiseTexture);
        }

        private Texture2D ReadNoiseTexture(Texture2D inputTexture)
        {
            _outputTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);

            int width = inputTexture.width;
            int height = inputTexture.width;
            for (int y = 0; y < height; y++)
            {
                for(int x = 0; x< width; x++)
                {
                    Debug.Log(inputTexture.GetPixel(x, y));
                    if(inputTexture.GetPixel(x,y).r < 0.2f)
                    {
                        _outputTexture.SetPixel(x, y, new Color(10, 10, 10, 255));
                    }
                    else
                    {
                        _outputTexture.SetPixel(x, y, new Color(100, 100, 100, 255));
                    }
                }
            }

            _outputTexture.Apply();
            _outputTexture.filterMode = FilterMode.Point;
            _outputTexture.wrapMode = TextureWrapMode.Clamp;
            return _outputTexture;
        }
    }
}
