using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using Newtonsoft.Json;

namespace Match3.LevelEditor
{
    public class WebGLExportBtn : MonoBehaviour, IPointerDownHandler
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        //
        // WebGL
        //
        [DllImport("__Internal")]
        private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

        // Broser plugin should be called in OnPointerDown.
        public void OnPointerDown(PointerEventData eventData)
        {
            LevelData levelData = GridManager.Instance.GetLevelData();
            string json = JsonConvert.SerializeObject(levelData);
            var bytes = Encoding.UTF8.GetBytes(json);

            string fileName = LevelEditorSaveManager.Instance.FileName;
            DownloadFile(gameObject.name, "OnFileDownload", fileName, bytes, bytes.Length);
        }

        // Called from browser
        public void OnFileDownload()
        {
            UILogHandler.Instance.ShowLogText($"Export successfully!!!", 5f);
        }
#else
    public void OnPointerDown(PointerEventData eventData) { }
#endif
    }

}
