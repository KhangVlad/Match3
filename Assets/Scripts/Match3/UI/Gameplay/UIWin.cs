using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    public class UIWin : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private Button _homeBtn;
        [SerializeField] private ParticleSystem _cofettiPS;
        [SerializeField] private UIWinStar[] _uiStars;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            _homeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                Loader.Load(Loader.Scene.Town);
            });

            // _replayBtn.onClick.AddListener(() =>
            // {
            //     AudioManager.Instance.PlayButtonSfx();

            //     Loader.Load(Loader.Scene.GameplayScene);
            // });

            // _nextBtn.onClick.AddListener(() =>
            // {
            //     AudioManager.Instance.PlayButtonSfx();

            //     int nextLevel = LevelManager.Instance.NextLevel();
            //     LevelManager.Instance.LoadLevelData(LevelManager.Instance.CharacterLevelData.CharacterID, nextLevel);
            //     Loader.Load(Loader.Scene.GameplayScene);
            // });

#if WEBGL_BUILD
            _homeBtn.gameObject.SetActive(false);
#endif
        }

        private void OnDestroy()
        {
            _homeBtn.onClick.RemoveAllListeners();

        }

        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;

            if (enable)
            {
                PlayWinUIAnimationsAndEffects();
            }
        }


        private void PlayWinUIAnimationsAndEffects()
        {
            StartCoroutine(PlayWinUIAnimationsAndEffectsCoroutine());
        }

        private IEnumerator PlayWinUIAnimationsAndEffectsCoroutine()
        {
            _cofettiPS.Play();
            yield return new WaitForSeconds(0.5f);

            GameplayManager.Instance.CheckCompleteAllQuests(out int star);
            if (star < 0 || star > _uiStars.Length)
            {
                Debug.LogError("Something wen wrong!!!");
                yield break;
            }
            for (int i = 0; i < star; i++)
            {
                AudioManager.Instance.PlayButtonSfx();
                _uiStars[i].ActiveStar();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
