// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;
// using Match3;
// using TMPro;
//
// public class UISettingManager : MonoBehaviour
// {
//     public Color backgroundColor = new Color(10.0f / 255.0f, 10.0f / 255.0f, 10.0f / 255.0f, 0.6f);
//     private Canvas _canvas;
//
//     [Header("Sounds Setting")] public GameObject m_background;
//     [SerializeField] private Animator _animatorSoundPanel;
//     [SerializeField] private Animator _animatorLanguagePanel;
//     [SerializeField] private Slider _musicSlider;
//     [SerializeField] private Slider _soundSlider;
//     [SerializeField] private Button _closeButton;
//     [SerializeField] private Button _openSoundSettingBtn;
//     [SerializeField] private Button _saveButton;
//     [SerializeField] private Transform settingParent;
//
//     [Header("Language Setting")] [SerializeField]
//     private Button _openLanguageSettingBtn;
//
//     [SerializeField] private Transform languageParent;
//     [SerializeField] private Button _closeLangueBtn;
//     [SerializeField] private Button _saveLangueBtn;
//     [SerializeField] private TMP_Dropdown _dropdownlanguage;
//
//     private void Awake()
//     {
//         _canvas = GetComponent<Canvas>();
//         LoadSettings();
//     }
//
//     private void Start()
//     {
//         _closeButton.onClick.AddListener(Close);
//         _openSoundSettingBtn.onClick.AddListener(OnOpenButtonClicked);
//         _openLanguageSettingBtn.onClick.AddListener(OnOpenLanguageButtonClicked);
//         _musicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
//         _soundSlider.onValueChanged.AddListener(OnSoundSliderValueChanged);
//         _saveButton.onClick.AddListener(OnSaveButtonClicked);
//         _closeLangueBtn.onClick.AddListener(OnCloseLanguageSetting);
//         _saveLangueBtn.onClick.AddListener(OnSaveLanguage);
//         _dropdownlanguage.onValueChanged.AddListener(OnLanguageChange);
//     }
//
//     private void OnLanguageChange(int value)
//     {
//         LanguageManager.Instance.ChangeLanguage((LanguageType)value);
//     }
//
//     private void OnCloseLanguageSetting()
//     {
//         AudioManager.Instance.PlayCloseBtnSfx();
//         languageParent.gameObject.SetActive(false);
//         m_background.gameObject.SetActive(false);
//     }
//
//     private void OnSaveLanguage()
//     {
//         AudioManager.Instance.PlayButtonSfx();
//         // Add logic to save the selected language
//         LanguageManager.Instance.ChangeLanguage((LanguageType)_dropdownlanguage.value);
//         languageParent.gameObject.SetActive(false);
//         m_background.gameObject.SetActive(false);
//     }
//
//     private void OnSaveButtonClicked()
//     {
//         AudioManager.Instance.PlayButtonSfx();
//         SaveSettings();
//         Close();
//     }
//
//     private void OnDestroy()
//     {
//         _closeButton.onClick.RemoveAllListeners();
//         _openSoundSettingBtn.onClick.RemoveAllListeners();
//         _openLanguageSettingBtn.onClick.RemoveAllListeners();
//         _musicSlider.onValueChanged.RemoveAllListeners();
//         _soundSlider.onValueChanged.RemoveAllListeners();
//         _closeLangueBtn.onClick.RemoveAllListeners();
//         _dropdownlanguage.onValueChanged.RemoveAllListeners();
//     }
//
//     private void OnOpenButtonClicked()
//     {
//         AudioManager.Instance.PlayButtonSfx();
//         CreateBackground();
//         settingParent.gameObject.SetActive(true);
//         if (_animatorSoundPanel.GetCurrentAnimatorStateInfo(0).IsName("Close"))
//             _animatorSoundPanel.Play("Open");
//     }
//
//     private void OnOpenLanguageButtonClicked()
//     {
//         AudioManager.Instance.PlayButtonSfx();
//         CreateBackground();
//         languageParent.gameObject.SetActive(true);
//         if (_animatorLanguagePanel.GetCurrentAnimatorStateInfo(0).IsName("Close"))
//             _animatorLanguagePanel.Play("Open");
//     }
//
//     private void CreateBackground()
//     {
//         m_background = new GameObject("Background");
//         var image = m_background.AddComponent(typeof(Image)) as Image;
//         image.color = backgroundColor;
//         image.canvasRenderer.SetAlpha(0.0f);
//         image.CrossFadeAlpha(1.0f, 0.4f, false);
//
//         m_background.transform.localScale = new Vector3(1, 1, 1);
//         m_background.GetComponent<RectTransform>().sizeDelta = _canvas.GetComponent<RectTransform>().sizeDelta;
//         m_background.transform.SetParent(_canvas.transform, false);
//         m_background.transform.SetSiblingIndex(1);
//     }
//
//     private void OnMusicSliderValueChanged(float value)
//     {
//         AudioManager.Instance.SetMasterVolume(value);
//     }
//
//     private void OnSoundSliderValueChanged(float value)
//     {
//         AudioManager.Instance.SetMasterVolume(value);
//     }
//
//     public void Close()
//     {
//         if (settingParent.gameObject.activeSelf && _animatorSoundPanel.GetCurrentAnimatorStateInfo(0).IsName("Open"))
//         {
//             _animatorSoundPanel.Play("Close");
//         }
//         else if (languageParent.gameObject.activeSelf &&
//                  _animatorLanguagePanel.GetCurrentAnimatorStateInfo(0).IsName("Open"))
//         {
//             _animatorLanguagePanel.Play("Close");
//         }
//
//         AudioManager.Instance.PlayCloseBtnSfx();
//         RemoveBackground();
//         StartCoroutine(RunPopupDestroy());
//     }
//
//     private IEnumerator RunPopupDestroy()
//     {
//         yield return new WaitForSeconds(0.5f);
//         m_background.gameObject.SetActive(false);
//     }
//
//     private void RemoveBackground()
//     {
//         var image = m_background.GetComponent<Image>();
//         if (image != null)
//             image.CrossFadeAlpha(0.0f, 0.2f, false);
//     }
//
//     private void SaveSettings()
//     {
//         PlayerPrefs.SetFloat("MusicVolume", _musicSlider.value);
//         PlayerPrefs.SetFloat("SoundVolume", _soundSlider.value);
//         PlayerPrefs.Save();
//     }
//
//     private void LoadSettings()
//     {
//         if (PlayerPrefs.HasKey("MusicVolume"))
//         {
//             _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
//         }
//
//         if (PlayerPrefs.HasKey("SoundVolume"))
//         {
//             _soundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
//         }
//     }
//
//     public void ActiveCanvas(bool active)
//     {
//         _canvas.enabled = active;
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Match3;
using TMPro;

public class UISettingManager : MonoBehaviour
{
    public Color backgroundColor = new Color(10.0f / 255.0f, 10.0f / 255.0f, 10.0f / 255.0f, 0.6f);
    private Canvas _canvas;

    [Header("Sounds Setting")] public GameObject m_background;
    [SerializeField] private Animator _animatorSoundPanel;
    [SerializeField] private Animator _animatorLanguagePanel;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _openSoundSettingBtn;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Transform settingParent;

    [Header("Language Setting")] [SerializeField]
    private Button _openLanguageSettingBtn;

    [SerializeField] private Transform languageParent;
    [SerializeField] private Button _closeLangueBtn;
    [SerializeField] private Button _saveLangueBtn;
    [SerializeField] private TMP_Dropdown _dropdownlanguage;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        LoadSettings();
    }

    private void Start()
    {
        _closeButton.onClick.AddListener(Close);
        _openSoundSettingBtn.onClick.AddListener(OnOpenButtonClicked);
        _openLanguageSettingBtn.onClick.AddListener(OnOpenLanguageButtonClicked);
        _musicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
        _soundSlider.onValueChanged.AddListener(OnSoundSliderValueChanged);
        _saveButton.onClick.AddListener(OnSaveButtonClicked);
        _closeLangueBtn.onClick.AddListener(OnCloseLanguageSetting);
        _saveLangueBtn.onClick.AddListener(OnSaveLanguage);
        _dropdownlanguage.onValueChanged.AddListener(OnLanguageChange);
    }

    private void OnLanguageChange(int value)
    {
        LanguageManager.Instance.ChangeLanguage((LanguageType)value);
    }

    private void OnCloseLanguageSetting()
    {
        AudioManager.Instance.PlayCloseBtnSfx();
        languageParent.gameObject.SetActive(false);
        m_background.SetActive(false);
    }

    private void OnSaveLanguage()
    {
        AudioManager.Instance.PlayButtonSfx();
        LanguageManager.Instance.ChangeLanguage((LanguageType)_dropdownlanguage.value);
        languageParent.gameObject.SetActive(false);
        m_background.SetActive(false);
    }

    private void OnSaveButtonClicked()
    {
        AudioManager.Instance.PlayButtonSfx();
        SaveSettings();
        Close();
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveAllListeners();
        _openSoundSettingBtn.onClick.RemoveAllListeners();
        _openLanguageSettingBtn.onClick.RemoveAllListeners();
        _musicSlider.onValueChanged.RemoveAllListeners();
        _soundSlider.onValueChanged.RemoveAllListeners();
        _closeLangueBtn.onClick.RemoveAllListeners();
        _dropdownlanguage.onValueChanged.RemoveAllListeners();
    }

    private void OnOpenButtonClicked()
    {
        AudioManager.Instance.PlayButtonSfx();
        CreateBackground();
        settingParent.gameObject.SetActive(true);
        if (_animatorSoundPanel.GetCurrentAnimatorStateInfo(0).IsName("Close"))
            _animatorSoundPanel.Play("Open");
    }

    private void OnOpenLanguageButtonClicked()
    {
        AudioManager.Instance.PlayButtonSfx();
        CreateBackground();
        languageParent.gameObject.SetActive(true);
        if (_animatorLanguagePanel.GetCurrentAnimatorStateInfo(0).IsName("Close"))
            _animatorLanguagePanel.Play("Open");
    }

    private void CreateBackground()
    {
        if (m_background == null)
        {
            m_background = new GameObject("Background");
            var image = m_background.AddComponent<Image>();
            image.color = backgroundColor;
            image.canvasRenderer.SetAlpha(0.0f);
            image.CrossFadeAlpha(1.0f, 0.4f, false);

            m_background.transform.localScale = new Vector3(1, 1, 1);
            m_background.GetComponent<RectTransform>().sizeDelta = _canvas.GetComponent<RectTransform>().sizeDelta;
            m_background.transform.SetParent(_canvas.transform, false);
            m_background.transform.SetSiblingIndex(1);
        }
        else
        {
            m_background.SetActive(true);
            var image = m_background.GetComponent<Image>();
            image.CrossFadeAlpha(1.0f, 0.4f, false);
        }
    }

    private void OnMusicSliderValueChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnSoundSliderValueChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
    }

    public void Close()
    {
        if (settingParent.gameObject.activeSelf && _animatorSoundPanel.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            _animatorSoundPanel.Play("Close");
        }
        else if (languageParent.gameObject.activeSelf &&
                 _animatorLanguagePanel.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            _animatorLanguagePanel.Play("Close");
        }

        AudioManager.Instance.PlayCloseBtnSfx();
        RemoveBackground();
        StartCoroutine(RunPopupDestroy());
    }

    private IEnumerator RunPopupDestroy()
    {
        yield return new WaitForSeconds(0.5f);
        m_background.SetActive(false);
    }

    private void RemoveBackground()
    {
        var image = m_background.GetComponent<Image>();
        if (image != null)
            image.CrossFadeAlpha(0.0f, 0.2f, false);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", _musicSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", _soundSlider.value);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }

        if (PlayerPrefs.HasKey("SoundVolume"))
        {
            _soundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        }
    }

    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
}