using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace Match3.LevelEditor
{
    [RequireComponent(typeof(Button))]
    public class WebGLOpenBtn : MonoBehaviour, IPointerDownHandler
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        //
        //   WEBGL
        //
        [DllImport("__Internal")]
        private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

        public void OnPointerDown(PointerEventData eventData)
        {
            UploadFile(gameObject.name, "OnFileUpload", ".json", false);
        }

        // Called from browser
        public void OnFileUpload(string url)
        {
            StartCoroutine(OutputRoutine(url));
        }

        private IEnumerator OutputRoutine(string url)
        {        
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;

                    LevelEditorManager.Instance.ShowCreateNewPanel = false;
                    LevelEditorSaveManager.Instance.LoadFromJson(json, onCompleted: () =>
                    {
                        // Callback logic if needed
                    });

                    //string fileName = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
                    string fileName = GetFileNameFromHeaders(request) ?? System.IO.Path.GetFileNameWithoutExtension(new System.Uri(url).LocalPath);
                    LevelEditorManager.Instance.SetFileName(fileName);
                    UIMenu.Instance.ChangeProjectName(fileName);
                    UIMenu.Instance.DisplayFilePopup(false);
                    UIMenu.Instance.DisplayCreateNewPanel(LevelEditorManager.Instance.ShowCreateNewPanel);
                    UILevelEditorManager.Instance.DisplayUILevelSelector(true);
                }
                else
                {
                    Debug.LogError("Error loading data: " + request.error);
                }
            }
        }

        private string GetFileNameFromHeaders(UnityWebRequest request)
        {
            string contentDisposition = request.GetResponseHeader("Content-Disposition");
            if (!string.IsNullOrEmpty(contentDisposition) && contentDisposition.Contains("filename="))
            {
                // Extract actual file name
                int index = contentDisposition.IndexOf("filename=") + 9;
                string fileName = contentDisposition.Substring(index).Trim('"');
                return System.IO.Path.GetFileNameWithoutExtension(fileName);
            }
            return null; // Fallback to URL-based extraction
        }
#else
        public void OnPointerDown(PointerEventData eventData) { }
#endif

    }

}
