using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;
using UnityEngine.SceneManagement;
using Match3.Shares;

namespace Match3.LevelEditor
{
    public class UIMenu : MonoBehaviour
    {
        public static UIMenu Instance { get; private set; }

        private Canvas _canvas;

        [Header("Tabs")]
        [SerializeField] private Button _fileBtn;
        [SerializeField] private Button _playBtn;


        [Header("File popup")]
        [SerializeField] private GameObject _filePopup;
        [SerializeField] private Button _closeFilePopupBtn;
        [SerializeField] private Button _newFileBtn;
        [SerializeField] private Button _saveAsFileBtn;
        [SerializeField] private Button _saveFileBtn;
        [SerializeField] private Button _openFileBtn;
        [SerializeField] private Button _exportFileBtn;
        [SerializeField] private Button _exitBtn;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _projectName;


        [Header("Others")]
        [SerializeField] private UIWarningModifiedPopup _uiWarningModifiedPopup;
        [SerializeField] private UICreateNewPanel _uiCreateNewPanel;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            _canvas = GetComponent<Canvas>();
        }


        private void Start()
        {
            //_projectName.text = BuilderSaveManager.Instance.GetProjectFileName();
            _filePopup.gameObject.SetActive(false);
            _uiWarningModifiedPopup.gameObject.SetActive(false);

            _uiCreateNewPanel.gameObject.SetActive(LevelEditorManager.Instance.ShowCreateNewPanel);

            _fileBtn.onClick.AddListener(() =>
            {
                _filePopup.gameObject.SetActive(true);
            });

            _playBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                LevelManager.Instance.SetCharacterLevelData(LevelEditorManager.Instance.CharacterLevelData);
                LevelDataV2 levelData = GridManager.Instance.GetLevelData();
                LevelManager.Instance.SetLevelData(levelData);
                //Loader.Load(Loader.Scene.GameplayScene);

                StartCoroutine(Loader.LoadSceneAsyncCoroutine(Loader.Scene.GameplayScene, LoadSceneMode.Additive, 0f, () =>
                {
                    LevelEditorSceneLoader.Instance.OtherScene = SceneManager.GetSceneByName(Loader.Scene.GameplayScene.ToString());
                    LevelEditorSceneLoader.Instance.DisplaySceneObject(false);
                }));
            });

            // File popup
            _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File*";
            _closeFilePopupBtn.onClick.AddListener(() =>
            {
                _filePopup.gameObject.SetActive(false);
            });
            _newFileBtn.onClick.AddListener(() =>
            {
                LevelEditorManager.Instance.ShowCreateNewPanel = true;

                _filePopup.gameObject.SetActive(false);
                Loader.Load(Loader.Scene.LevelEditor);
                _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File*";
            });
#if false
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
#endif

#if !(UNITY_WEBGL && !UNITY_EDITOR)
            //
            // Standalone platforms & editor
            //
            _openFileBtn.onClick.AddListener(() =>
            {
                LevelEditorManager.Instance.ShowCreateNewPanel = false;
                var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "json", false);
                if (paths.Length > 0)
                {
                    LevelEditorSaveManager.Instance.Load(paths[0], onCompleted: () =>
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(paths[0]);
                        LevelEditorManager.Instance.SetFileName(fileName);
                        _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File";
                        _saveFileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save";
                        _projectName.text = fileName;
                    });
                }
                _filePopup.gameObject.SetActive(false);
                _uiCreateNewPanel.gameObject.SetActive(LevelEditorManager.Instance.ShowCreateNewPanel);

            });

            _exportFileBtn.onClick.AddListener(() =>
            {
                var path = StandaloneFileBrowser.SaveFilePanel("Title", "", LevelEditorManager.Instance.FileName, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    LevelEditorSaveManager.Instance.SaveAs(path);
                    _projectName.text = LevelEditorSaveManager.Instance.GetProjectFileName();

                    _fileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "File";
                    _saveFileBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save";
                }
            });
#endif



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
                            //LevelEditorSaveManager.Instance.Save();
                            QuitGame();
                        }
                        else
                        {
                            //FileBrowserManager.Instance.ShowSaveDialog((filePaths) =>
                            //{
                            //    if (filePaths != null && filePaths.Length > 0)
                            //    {
                            //        LevelEditorSaveManager.Instance.SaveAs(filePaths[0]);
                            //        QuitGame();
                            //    }
                            //});
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
            _playBtn.onClick.RemoveAllListeners();

            // File popup
            _closeFilePopupBtn.onClick.RemoveAllListeners();
            _newFileBtn.onClick.RemoveAllListeners();
            _saveAsFileBtn.onClick.RemoveAllListeners();
            _saveFileBtn.onClick.RemoveAllListeners();
            _openFileBtn.onClick.RemoveAllListeners();
            _exitBtn.onClick.RemoveAllListeners();
            _exportFileBtn.onClick.RemoveAllListeners();
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



        public void ChangeProjectName(string name)
        {
            this._projectName.text = name;
        }
        public void DisplayFilePopup(bool enable)
        {
            _filePopup.gameObject.SetActive(enable);
        }
        public void DisplayCreateNewPanel(bool enable)
        {
            _uiCreateNewPanel.gameObject.SetActive(enable);
        }




        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }
    }
}
