using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Match3.Shares;
using UnityEngine.SceneManagement;

namespace Match3
{
    public class UIWin : MonoBehaviour
    {
        private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _homeBtn;
        [SerializeField] private ParticleSystem _cofettiPS;
        [SerializeField] private UIWinStar[] _uiStars;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            _levelText.text =  $"Level  {LevelManager.Instance.CurrentLevelIndex + 1}";
            _homeBtn.onClick.AddListener(() =>
            {
                // AudioManager.Instance.PlayButtonSfx();
                // Loader.Load(Loader.Scene.Town);
                LevelManager.Instance.ActiveGameObject(true);
                SceneManager.UnloadSceneAsync(LevelManager.Instance.OtherScene);
            });

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
