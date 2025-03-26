using System;
using UnityEngine;

namespace Match3.LevelEditor
{
    public class CameraController : MonoBehaviour
    {
        private void Start()
        {
            GridManager.Instance.OnGridLoaded += OnGridLoaded_UpdateCamera;
        }

        private void OnDestroy()
        {
            GridManager.Instance.OnGridLoaded -= OnGridLoaded_UpdateCamera;
        }


        private void OnGridLoaded_UpdateCamera()
        {
            int width = GridManager.Instance.Width;
            int height = GridManager.Instance.Height;

            float offsetX = width * (TileExtension.TILE_WIDTH + TileExtension.SPACING_X) / 2f;
            float offsetY = height * (TileExtension.TILE_HEIGHT + TileExtension.SPACING_Y) / 2f;

            transform.position += new Vector3(offsetX, offsetY, 0);

            float size = Mathf.Max(5, Mathf.Max(width, height) + 0.5f);
            Camera.main.orthographicSize = size;
        }

    }
}
