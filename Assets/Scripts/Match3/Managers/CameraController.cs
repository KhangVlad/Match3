using Codice.Client.BaseCommands;
using UnityEngine;


namespace Match3
{
    public class CameraController : MonoBehaviour
    {
        public static event System.Action<Vector3> OnCameraPositionUpdated;
        private float targetSize;
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
            targetSize = Camera.main.orthographicSize;
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            UpdateCameraScroll();
#endif
        }



        private void UpdateCameraScroll()
        {
            int minSize = 5;
            float maxSize = 50f;
            float scrollSpeed = 10f;     // How much scroll affects targetSize
            float smoothSpeed = 5f;      // How quickly camera moves to targetSize

            if (Camera.main == null)
                return;

            float scrollInput = Input.GetAxis("Mouse ScrollWheel");


            if (scrollInput != 0f)
            {
                targetSize -= scrollInput * scrollSpeed;
                targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
            }

            // Smoothly move current size toward target size
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetSize, Time.deltaTime * smoothSpeed);
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
            GetComponent<Camera>().orthographicSize = size;
            OnCameraPositionUpdated?.Invoke(transform.position);
        }
    }
}