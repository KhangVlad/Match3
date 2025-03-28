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
            //var loader = new WWW(url);
            //yield return loader;
            ////output.text = loader.text;

            //EditorManager.Instance.ShowCreateNewPanel = false;
            //LevelEditorSaveManager.Instance.LoadFromJson(loader.text, onCompleted: () =>
            //{

            //});
            //UIMenu.Instance.DisplayFilePopup(false);
            //UIMenu.Instance.DisplayCreateNewPanel(EditorManager.Instance.ShowCreateNewPanel);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;

                    EditorManager.Instance.ShowCreateNewPanel = false;
                    LevelEditorSaveManager.Instance.LoadFromJson(json, onCompleted: () =>
                    {
                        // Callback logic if needed
                    });

                    UIMenu.Instance.DisplayFilePopup(false);
                    UIMenu.Instance.DisplayCreateNewPanel(EditorManager.Instance.ShowCreateNewPanel);
                }
                else
                {
                    Debug.LogError("Error loading data: " + request.error);
                }
            }
        }
#else
        public void OnPointerDown(PointerEventData eventData) { }
#endif

    }

}
