using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Match3;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

#if !UNITY_WEBGL
public class UISettingManager : MonoBehaviour
{
    public Color backgroundColor = new Color(10.0f / 255.0f, 10.0f / 255.0f, 10.0f / 255.0f, 0.6f);
    private Canvas _canvas;

    [Header("Sounds Setting")] public GameObject m_background;
    [SerializeField] private Animator _animatorSoundPanel;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _openSoundSettingBtn;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Transform settingParent;
    [SerializeField] private TMP_Dropdown _dropdownlanguage;
    [SerializeField] private TMP_Dropdown _linkCredentialOptions; //google or ch play 0 is google, 1 is ch play
    

    [Header("Temp Variables")] [SerializeField]
    private Button _dayAndNightToggle; //get child image to change sprite

    private TimeOfDay _timeOfDay;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        LoadSettings();
    }

    private void Start()
    {
        _closeButton.onClick.AddListener(Close);
        _openSoundSettingBtn.onClick.AddListener(OnOpenButtonClicked);
        _musicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
        _soundSlider.onValueChanged.AddListener(OnSoundSliderValueChanged);
        _saveButton.onClick.AddListener(OnSaveButtonClicked);
        _dropdownlanguage.onValueChanged.AddListener(OnLanguageChange);
        _dayAndNightToggle.onClick.AddListener(OnDayAndNightToggle);
        
        // Update credential options visibility based on user data
        UpdateCredentialOptionsVisibility();
        
        // Add listener for link credential dropdown
        if (_linkCredentialOptions != null)
        {
            _linkCredentialOptions.onValueChanged.AddListener(OnLinkCredentialOptionChanged);
        }
        
        _animatorSoundPanel.Play("Close");
    }
    
    private void UpdateCredentialOptionsVisibility()
    {
        if (UserManager.Instance != null && UserManager.Instance.UserData != null)
        {
            // Show the dropdown if the account is a Guest account, hide it otherwise
            bool isGuest = UserManager.Instance.UserData.LinkedCredential == "Guest";
            if (_linkCredentialOptions != null)
            {
                _linkCredentialOptions.gameObject.SetActive(isGuest);
            }
        }
    }
    
    private void OnLinkCredentialOptionChanged(int optionIndex)
    {
        AudioManager.Instance.PlayButtonSfx();
        
        // 0 is Google, 1 is CH Play
        if (optionIndex == 0)
        {
            if (AuthenticationManager.Instance != null)
            {
                AuthenticationManager.Instance.HandleGoogleSignIn();
            }
        }
        else if (optionIndex == 1)
        {
            if (AuthenticationManager.Instance != null)
            {
                StartCoroutine(AuthenticationManager.Instance.HandleChPlaySignIn(() => {
                    // Update UI after linking
                    UpdateCredentialOptionsVisibility();
                }));
            }
        }
    }
    
    private void OnDayAndNightToggle()
    {
        if (LightManager.Instance == null)
        {
            Debug.LogError("LightManager instance is null!");
            return;
        }

        AudioManager.Instance.PlayButtonSfx();
        LightManager.Instance.ToggleDayNight();
    }

    private void OnLanguageChange(int value)
    {
        LanguageManager.Instance.ChangeLanguage((LanguageType)value);
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
        _musicSlider.onValueChanged.RemoveAllListeners();
        _soundSlider.onValueChanged.RemoveAllListeners();
        _dropdownlanguage.onValueChanged.RemoveAllListeners();
        _saveButton.onClick.RemoveAllListeners();
        _dayAndNightToggle.onClick.RemoveAllListeners();
        
        if (_linkCredentialOptions != null)
        {
            _linkCredentialOptions.onValueChanged.RemoveAllListeners();
        }
    }

    private void OnOpenButtonClicked()
    {
        AudioManager.Instance.PlayButtonSfx();
        CreateBackground();
        settingParent.gameObject.SetActive(true);
        
        // Update credential options visibility when opening settings
        UpdateCredentialOptionsVisibility();
        
        if (_animatorSoundPanel.GetCurrentAnimatorStateInfo(0).IsName("Close"))
            _animatorSoundPanel.Play("Open");
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
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSoundSliderValueChanged(float value)
    {
        AudioManager.Instance.SetSoundVolume(value);
    }

    public void Close()
    {
        if (settingParent.gameObject.activeSelf && _animatorSoundPanel.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            _animatorSoundPanel.Play("Close");
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
#endif
