
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Match3;
using DG.Tweening;

public class UISettingManager : MonoBehaviour
{
    public Color backgroundColor = new Color(10.0f / 255.0f, 10.0f / 255.0f, 10.0f / 255.0f, 0.6f);
    private GameObject m_background;
    private Canvas _canvas;
    [SerializeField] private Animator _animator;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _openSettingButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Transform settingParent;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        LoadSettings();
    }

    private void Start()
    {
        _closeButton.onClick.AddListener(Close);
        _openSettingButton.onClick.AddListener(OnOpenButtonClicked);
        _musicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
        _soundSlider.onValueChanged.AddListener(OnSoundSliderValueChanged);
        _saveButton.onClick.AddListener(OnSaveButtonClicked);
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
        _openSettingButton.onClick.RemoveAllListeners();
        _musicSlider.onValueChanged.RemoveAllListeners();
        _soundSlider.onValueChanged.RemoveAllListeners();
    }

    private void OnOpenButtonClicked()
    {
        AudioManager.Instance.PlayButtonSfx();
        var bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, backgroundColor);
        bgTex.Apply();

        m_background = new GameObject("PopupBackground");
        var image = m_background.AddComponent<Image>();
        var rect = new Rect(0, 0, bgTex.width, bgTex.height);
        var sprite = Sprite.Create(bgTex, rect, new Vector2(0.5f, 0.5f), 1);
        image.material.mainTexture = bgTex;
        image.sprite = sprite;
        var newColor = image.color;
        image.color = newColor;
        image.canvasRenderer.SetAlpha(0.0f);
        image.CrossFadeAlpha(1.0f, 0.4f, false);

        m_background.transform.localScale = new Vector3(1, 1, 1);
        m_background.GetComponent<RectTransform>().sizeDelta = _canvas.GetComponent<RectTransform>().sizeDelta;
        m_background.transform.SetParent(_canvas.transform, false);
        m_background.transform.SetSiblingIndex(1);
        settingParent.gameObject.SetActive(true);
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
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
            _animator.Play("Close");
        AudioManager.Instance.PlayCloseBtnSfx();
        RemoveBackground();
        StartCoroutine(RunPopupDestroy());
    }

    private IEnumerator RunPopupDestroy()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(m_background);
        settingParent.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => settingParent.gameObject.SetActive(false));
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