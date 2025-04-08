using UnityEngine;


namespace Match3
{
    public class CameraController : MonoBehaviour
    {
        private void Start()
        {
            //LevelData levelData = LevelManager.Instance.LevelData;
            //int width = levelData.Blocks.GetLength(0);
            //int height = levelData.Blocks.GetLength(1);

            //Debug.Log($"{width}  {height}");

            //switch (width)
            //{
            //    case 7:
            //        Camera.main.transform.position = new Vector3(3.8f, 6f, -10);
            //        Camera.main.orthographicSize = 8.5f;
            //        break;
            //    case 8:
            //        Camera.main.transform.position = new Vector3(4.5f, 6f, -10);
            //        Camera.main.orthographicSize = 9.5f;
            //        break;
            //    case 9:
            //        Camera.main.transform.position = new Vector3(4.9f, 6f, -10);
            //        Camera.main.orthographicSize = 11f;
            //        break;
            //}

            //Debug.Log("AAAA"+gameObject.name);

            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            LevelDataV2 levelData = LevelManager.Instance.LevelData;
            int width = levelData.Blocks.GetLength(0);
            int height = levelData.Blocks.GetLength(1);

            float offsetX = width * (TileExtension.TILE_WIDTH + TileExtension.SPACING_X) / 2f;
            float offsetY = height * (TileExtension.TILE_HEIGHT + TileExtension.SPACING_Y) / 2f;

            transform.position = new Vector3(0, 0, -10);
            transform.position += new Vector3(offsetX, offsetY, 0);

            float size = Mathf.Max(5, Mathf.Max(width, height) + 0.5f);
            Camera.main.orthographicSize = size;
        }
    }
}