using System;
using Match3;
using UnityEngine;
using Match3.Shares;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }
    public LanguageType currentLanguage;

    public event Action<LanguageType> OnLanguageChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Utilities.WaitAfter(0.1f, LoadLanguage);
    }

    public void ChangeLanguage(LanguageType newLanguage)
    {
        currentLanguage = newLanguage;
        SaveLanguage();
    }

    private void SaveLanguage()
    {
        PlayerPrefs.SetInt("Language", (int)currentLanguage);
        PlayerPrefs.Save();
    }

    private void LoadLanguage()
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            currentLanguage = (LanguageType)PlayerPrefs.GetInt("Language");
        }
        else
        {
            currentLanguage = LanguageType.EN;
        }
    }
}