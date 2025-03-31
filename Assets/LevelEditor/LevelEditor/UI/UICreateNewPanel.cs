using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.LevelEditor
{
    public class UICreateNewPanel : MonoBehaviour
    {
        [Header("InputFileds")]
        [SerializeField] private TMP_InputField _fileNameInputField;
  

        [Header("Buttons")]
        [SerializeField] private Button _cancelBtn;
        [SerializeField] private Button _okBtn;

        private void Start()
        {
            _okBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelEditorManager.Instance.SetFileName(_fileNameInputField.text);
                LevelEditorManager.Instance.InitializeNewChartacterLevelData();

                this.gameObject.SetActive(false);
            });

            _cancelBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                this.gameObject.SetActive(false);
            });

            _fileNameInputField.Select();
            _fileNameInputField.ActivateInputField();
        }



     


        private void OnDestroy()
        {
            _okBtn.onClick.RemoveAllListeners();
            _cancelBtn.onClick.RemoveAllListeners();
        }
    }
}
