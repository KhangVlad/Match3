using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Match3.LevelEditor
{
    public class UILogHandler : MonoBehaviour
    {
        public static UILogHandler  Instance { get; private set; }   

        [SerializeField] private TextMeshProUGUI _warningText;
        [SerializeField] private TextMeshProUGUI _logText;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

        }

        private void Start()
        {
            _warningText.gameObject.SetActive(false);
            _logText.gameObject.SetActive(false);
        }

        public void ShowWarningText(string warningText, float duration = 5f)
        {
            _warningText.gameObject.SetActive(true);
            _warningText.text = $"Warning: {warningText}";

            StartCoroutine(WaitAfterCoroutine(duration, ()=>
            {
                _warningText.gameObject.SetActive(false);
            })); 
        }

        public void ShowLogText(string logText, float duration = 5f)
        {
            _logText.gameObject.SetActive(true);
            _logText.text = $"Log: {logText}";

            StartCoroutine(WaitAfterCoroutine(duration, () =>
            {
                _logText.gameObject.SetActive(false);
            }));
        }


        private IEnumerator WaitAfterCoroutine(float delay, System.Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
    }
}
