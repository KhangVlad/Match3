using UnityEngine;


namespace Match3
{
    public class CameraController : MonoBehaviour
    {
        private void Start()
        {
            LevelData levelData = LevelManager.Instance.LevelData;
            int width = levelData.Data.GetLength(0);
            int height = levelData.Data.GetLength(1);

            Debug.Log($"{width}  {height}");

            switch (width)
            {
                case 7:
                    Camera.main.transform.position = new Vector3(3.8f, 6f, -10);
                    Camera.main.orthographicSize = 8.5f;
                    break;
                case 8:
                    Camera.main.transform.position = new Vector3(4.5f, 6f, -10);
                    Camera.main.orthographicSize = 9.5f;
                    break;
                case 9:
                    Camera.main.transform.position = new Vector3(4.9f, 6f, -10);
                    Camera.main.orthographicSize = 11f;
                    break;
            }

            Debug.Log("AAAA"+gameObject.name);
        }
    }
}