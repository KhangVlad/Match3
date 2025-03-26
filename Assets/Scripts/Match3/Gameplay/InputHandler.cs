using UnityEngine;

namespace Match3
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }


        public Vector2Int GetGridPositionByMouse()
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return GetGridPosition(worldPosition);
        }


        public Vector2Int GetGridPosition(Vector2 worldPosition)
        {
            int gridX = Mathf.FloorToInt(worldPosition.x / (TileExtension.TILE_WIDTH + TileExtension.SPACING_X));
            int gridY = Mathf.FloorToInt(worldPosition.y / (TileExtension.TILE_HEIGHT + TileExtension.SPACING_Y));
            return new Vector2Int(gridX, gridY);
        }
    }
}
