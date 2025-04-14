using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    public class UICanvasMap : MonoBehaviour
    {
        [SerializeField] private Button _level1Btn;
        [SerializeField] private Button _level2Btn;
        [SerializeField] private Button _level3Btn;
        [SerializeField] private Button _level4Btn;
        [SerializeField] private Button _level5Btn;

        [SerializeField] private Button _shopBtn;

        private void Start()
        {
            //Loader.Load(Loader.Scene.GameplayScene);
            _level1Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelManager.Instance.LoadLevelData(1);

                UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData, LevelManager.Instance.CurrentLevelIndex);
                UILevelInfomation.Instance.DisplayCanvas(true);

            });

            _level2Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelManager.Instance.LoadLevelData(2);

                UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData, LevelManager.Instance.CurrentLevelIndex);
                UILevelInfomation.Instance.DisplayCanvas(true);
            });

            _level3Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelManager.Instance.LoadLevelData(3);

                UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData, LevelManager.Instance.CurrentLevelIndex);
                UILevelInfomation.Instance.DisplayCanvas(true);
            });

            _level4Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelManager.Instance.LoadLevelData(4);

                UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData, LevelManager.Instance.CurrentLevelIndex);
                UILevelInfomation.Instance.DisplayCanvas(true);
            });

            _level5Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelManager.Instance.LoadLevelData(5);

                UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData, LevelManager.Instance.CurrentLevelIndex);
                UILevelInfomation.Instance.DisplayCanvas(true);
            });

            _shopBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                UIHomeManager.Instance.DisplayUIShop(true);
            });
        }

        private void OnDestroy()
        {
            _level1Btn.onClick.RemoveAllListeners();
            _level2Btn.onClick.RemoveAllListeners();
            _level3Btn.onClick.RemoveAllListeners();
            _level4Btn.onClick.RemoveAllListeners();
            _level5Btn.onClick.RemoveAllListeners();

            _shopBtn.onClick.RemoveAllListeners();
        }

    }

}
