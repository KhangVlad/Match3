using System;
using UnityEngine;

namespace Match3.LevelEditor
{
    public class UILevelEditor : MonoBehaviour
    {
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            GridManager.Instance.OnGridLoaded += OnGridLoaded_UpdateUI;
        }

   
        private void OnDestroy()
        {
            GridManager.Instance.OnGridLoaded -= OnGridLoaded_UpdateUI;
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        private void OnGridLoaded_UpdateUI()
        {
           DisplayCanvas(true); 
        }

    }
}
