using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Match3.LevelEditor
{
    public class UIMenu : MonoBehaviour
    {
        private Canvas _canvas;

        [Header("Tabs")]
        [SerializeField] private Button _fileBtn;


        [Header("File popup")]
        [SerializeField] private GameObject _filePopup;
        [SerializeField] private Button _closeFilePopupBtn;
        [SerializeField] private Button _newFileBtn;
        [SerializeField] private Button _saveAsFileBtn;
        [SerializeField] private Button _saveFileBtn;
        [SerializeField] private Button _openFileBtn;
        [SerializeField] private Button _exitBtn;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _projectName;

  
        [Header("Others")]
        [SerializeField] private UIWarningModifiedPopup _uiWarningModifiedPopup;
        [SerializeField] private UICreateNewPanel _uiCreateNewPanel;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }


        private void Start()
        {
            //_projectName.text = BuilderSaveManager.Instance.GetProjectFileName();
            _filePopup.gameObject.SetActive(false);
            _uiWarningModifiedPopup.gameObject.SetActive(false);

            _uiCreateNewPanel.gameObject.SetActive(EditorManager.Instance.ShowCreateNewPanel);

            _fileBtn.onClick.AddListener(() =>
            {
                _filePopup.gameObject.SetActive(true);
            });



            // File popup
            _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File*";
            _closeFilePopupBtn.onClick.AddListener(() =>
            {
                _filePopup.gameObject.SetActive(false);
            });
            _newFileBtn.onClick.AddListener(() =>
            {
                EditorManager.Instance.ShowCreateNewPanel = true;

                _filePopup.gameObject.SetActive(false);
                Loader.Load(Loader.Scene.LevelEditor);
                _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File*";
            });
            _saveFileBtn.onClick.AddListener(() =>
            {
                if (LevelEditorSaveManager.Instance.IsCurrentWorkingFileExist())
                {
                    LevelEditorSaveManager.Instance.Save();
                    _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File";
                    _saveFileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save";
                    _projectName.text = LevelEditorSaveManager.Instance.GetProjectFileName();
                }
                else
                {
                    FileBrowserManager.Instance.ShowSaveDialog((filePaths) =>
                    {
                        if (filePaths != null && filePaths.Length > 0)
                        {
                            LevelEditorSaveManager.Instance.SaveAs(filePaths[0]);

                            _filePopup.gameObject.SetActive(false);
                            _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File";
                            _saveFileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save";
                            _projectName.text = LevelEditorSaveManager.Instance.GetProjectFileName();
                        }
                    });
                }
                _filePopup.gameObject.SetActive(false);
            });
            _saveAsFileBtn.onClick.AddListener(() =>
            {
                _filePopup.gameObject.SetActive(false);
                FileBrowserManager.Instance.ShowSaveDialog((filePaths) =>
                {
                    if (filePaths != null && filePaths.Length > 0)
                    {
                        LevelEditorSaveManager.Instance.SaveAs(filePaths[0]);
                        _projectName.text = LevelEditorSaveManager.Instance.GetProjectFileName();

                        _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File";
                        _saveFileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save";
                    }
                });
            });
            _openFileBtn.onClick.AddListener(() =>
            {
                EditorManager.Instance.ShowCreateNewPanel = false;

                Loader.Load(Loader.Scene.LevelEditor);
                FileBrowserManager.Instance.ShowLoadDialog((isSuccess, filePaths) =>
                {
                    if (filePaths != null && filePaths.Length > 0)
                        LevelEditorSaveManager.Instance.Load(filePaths[0], onCompleted: () =>
                        {
                            _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File";
                            _saveFileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save";
                            _projectName.text = LevelEditorSaveManager.Instance.GetProjectFileName();
                        });
                });
                _filePopup.gameObject.SetActive(false);

            });
            _exitBtn.onClick.AddListener(() =>
            {
                _filePopup.gameObject.SetActive(false);

                if (LevelEditorSaveManager.Instance.HasBeenSaved)
                {
                    QuitGame();
                }
                else
                {
                    _uiWarningModifiedPopup.gameObject.SetActive(true);


                    _uiWarningModifiedPopup.OnSaveBtnClick(() =>
                    {
                        if (LevelEditorSaveManager.Instance.IsCurrentWorkingFileExist())
                        {
                            LevelEditorSaveManager.Instance.Save();
                            QuitGame();
                        }
                        else
                        {
                            FileBrowserManager.Instance.ShowSaveDialog((filePaths) =>
                            {
                                if (filePaths != null && filePaths.Length > 0)
                                {
                                    LevelEditorSaveManager.Instance.SaveAs(filePaths[0]);
                                    QuitGame();
                                }
                            });
                        }
                    });

                    _uiWarningModifiedPopup.OnCancelBtnClick(() =>
                    {
                        _uiWarningModifiedPopup.gameObject.SetActive(false);
                    });

                    _uiWarningModifiedPopup.OnDontSaveBtnClick(() =>
                    {
                        QuitGame();
                    });
                }
            });
        }



        private void OnDestroy()
        {
            _fileBtn.onClick.RemoveAllListeners();

            // File popup
            _closeFilePopupBtn.onClick.RemoveAllListeners();
            _newFileBtn.onClick.RemoveAllListeners();
            _saveAsFileBtn.onClick.RemoveAllListeners();
            _saveFileBtn.onClick.RemoveAllListeners();
            _openFileBtn.onClick.RemoveAllListeners();
            _exitBtn.onClick.RemoveAllListeners();
        }



        private void OnDataHasChanged_MarkUI()
        {
            _projectName.text = $"{LevelEditorSaveManager.Instance.GetProjectFileName()}*";
            _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File*";
            _saveFileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save*";
        }
        private void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;    // stop play mode in Unity Editor
#endif
        }

        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }
    }
}
