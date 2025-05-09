using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Match3;
using TMPro;
using UnityEngine.UI;

#if !UNITY_WEBGL
public class SpinWheel : MonoBehaviour
{
    public AnimationCurve AnimationCurve;
    [SerializeField] private RectTransform spinWheelTransform;
    [SerializeField] private Button spinButton;
    [SerializeField] private RectTransform handle_bar_anim;
    private int defaultItem = 5;
    public SpinItem itemPrefab;
    public List<SpinItem> allItem;
    public int ItemCount = 5;
    private bool m_spinning = false;
    private int selectedItemIndex = -1;
    private float lastAngle = 0f;
    public int resultIndex;

    [SerializeField] private Transform rewardPanel;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI timeNextSpin;
    [SerializeField] private Animator light_bulb_anim;
    [SerializeField] private Image clocklIcon;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite InActiveSprite;
    [SerializeField] private ParticleSystem confestyPrefab;
    private float animation_Speed_Deffault = 1;
    private float animation_Max_Speed = 5;

    private TimeSpan timeSinceLastSpin;

    // Added variables for spin cooldown
    private const int HOURS_BETWEEN_SPINS = 12;
    private bool canSpin = false;

    private void Start()
    {
        spinButton.onClick.AddListener(Spin);
        InitializeSpinWheelItem();
        InitializeTimeText();
    }

    private void Update()
    {
        canSpin = IsSpinAvailable();
        UpdateNextSpinText();
        spinButton.interactable = canSpin && !m_spinning;

        //for debug
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(DoSpin());
        }
    }

    private void OnDestroy()
    {
        spinButton.onClick.RemoveListener(Spin);
    }

    private void InitializeTimeText()
    {
        timeSinceLastSpin = TimeManager.Instance.ServerTime - UserManager.Instance.UserData.SpinTime.StringToDateTime();
        TimeSpan timeRemaining = TimeSpan.FromHours(HOURS_BETWEEN_SPINS) - timeSinceLastSpin;
        if (timeRemaining.TotalSeconds > 0)
        {
            clocklIcon.enabled = true;
            buttonImage.sprite = InActiveSprite;
        }
        else
        {
            clocklIcon.enabled = false;
        }
        string formattedTime = string.Format("{0:D2}:{1:D2}",
            timeRemaining.Hours,
            timeRemaining.Minutes);
        timeNextSpin.text = $"{formattedTime}";
    }

    private bool IsSpinAvailable()
    {
        if (!TimeManager.Instance.IsTimeValid)
        {
            return false;
        }

        return timeSinceLastSpin.TotalHours >= HOURS_BETWEEN_SPINS;
    }

    private void UpdateNextSpinText()
    {
        timeSinceLastSpin = TimeManager.Instance.ServerTime - UserManager.Instance.UserData.SpinTime.StringToDateTime();
        if (timeSinceLastSpin.TotalHours >= HOURS_BETWEEN_SPINS)
        {
            timeNextSpin.text = "Spin Now!";
            return;
        }

        TimeSpan timeRemaining = TimeSpan.FromHours(HOURS_BETWEEN_SPINS) - timeSinceLastSpin;
        if (timeRemaining.TotalSeconds <= 0)
        {
            timeNextSpin.text = "Spin Now!";
            return;
        }

        // Format and display remaining time
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
            timeRemaining.Hours,
            timeRemaining.Minutes,
            timeRemaining.Seconds);

        timeNextSpin.text = $"{formattedTime}";
    }

    public void Spin()
    {
        if (!IsSpinAvailable())
        {
            Debug.Log("Cannot spin yet. Wait for cooldown to finish.");
            return;
        }

        // Update the last spin time to current server time
        UserManager.Instance.UserData.SpinTime = TimeManager.Instance.ServerTime.ToString();
        UserManager.Instance.ClaimDailyReward(BoosterID.Hammer, 1);

        canSpin = false;
        if (!m_spinning)
            StartCoroutine(DoSpin());
    }

    // private IEnumerator DoSpin()
    // {
    //     m_spinning = true;
    //     spinButton.interactable = false;
    //     float angleStep = 360f / ItemCount;
    //     float extraRotations = 3; // Adds extra rotations before stopping
    //     float targetAngle = -(resultIndex * angleStep + (extraRotations * 360));
    //
    //     float duration = 3.0f; // Spin duration
    //     float elapsed = 0f;
    //     float startAngle = spinWheelTransform.eulerAngles.z;
    //     lastAngle = startAngle;
    //
    //     while (elapsed < duration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = elapsed / duration;
    //         float curveValue = AnimationCurve.Evaluate(t); // Smooth curve effect
    //         float newAngle = Mathf.Lerp(startAngle, targetAngle, curveValue);
    //         spinWheelTransform.eulerAngles = new Vector3(0, 0, newAngle);
    //         CheckAngleChangeForShake(newAngle);
    //
    //         yield return null;
    //     }
    //
    //     spinWheelTransform.eulerAngles = new Vector3(0, 0, targetAngle % 360);
    //     Debug.Log($"{(BoosterID)resultIndex + 1}");
    //     m_spinning = false;
    //
    //     ShowRewardPanel();
    // }
    private IEnumerator DoSpin()
    {
        m_spinning = true;
        spinButton.interactable = false;
        float angleStep = 360f / ItemCount;
        float extraRotations = 3; // Adds extra rotations before stopping
        float targetAngle = -(resultIndex * angleStep + (extraRotations * 360));

        float duration = 3.0f; // Spin duration
        float elapsed = 0f;
        float startAngle = spinWheelTransform.eulerAngles.z;
        lastAngle = startAngle;

        // Set animator parameter to default speed initially
        if (light_bulb_anim != null)
        {
            light_bulb_anim.SetFloat("Speed", animation_Speed_Deffault);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = AnimationCurve.Evaluate(t); // Smooth curve effect
            float newAngle = Mathf.Lerp(startAngle, targetAngle, curveValue);
            spinWheelTransform.eulerAngles = new Vector3(0, 0, newAngle);
            CheckAngleChangeForShake(newAngle);

            // Update animator speed parameter based on elapsed time
            if (light_bulb_anim != null)
            {
                // Calculate speed between default and max based on elapsed time
                // First half: speed increases, second half: speed decreases
                float speedMultiplier;
                if (t <= 0.5f)
                {
                    // 0 to 0.5 -> increase from default to max
                    speedMultiplier = Mathf.Lerp(animation_Speed_Deffault, animation_Max_Speed, t * 2);
                }
                else
                {
                    // 0.5 to 1.0 -> decrease from max back to default
                    speedMultiplier = Mathf.Lerp(animation_Max_Speed, animation_Speed_Deffault, (t - 0.5f) * 2);
                }

                light_bulb_anim.SetFloat("Speed", speedMultiplier);
            }

            yield return null;
        }

        spinWheelTransform.eulerAngles = new Vector3(0, 0, targetAngle % 360);
        Debug.Log($"{(BoosterID)resultIndex + 1}");
        m_spinning = false;

        // Reset animator parameter back to default speed when done
        if (light_bulb_anim != null)
        {
            light_bulb_anim.SetFloat("Speed", animation_Speed_Deffault);
        }

        ShowRewardPanel();
    }

    private void CheckAngleChangeForShake(float newAngle)
    {
        // Normalize angles to 0-360 range
        float normalizedLastAngle = ((lastAngle % 360) + 360) % 360;
        float normalizedNewAngle = ((newAngle % 360) + 360) % 360;

        // Calculate the angle step based on item count
        float angleStep = 360f / ItemCount;

        // Check if we've crossed a section boundary
        int lastSection = Mathf.FloorToInt(normalizedLastAngle / angleStep);
        int newSection = Mathf.FloorToInt(normalizedNewAngle / angleStep);

        if (lastSection != newSection)
        {
            PlayShakeAnimation();
        }

        lastAngle = newAngle;
    }

    private void PlayShakeAnimation()
    {
        if (handle_bar_anim != null)
        {
            handle_bar_anim.DORotate(new Vector3(0, 0, 15f), 0.2f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    handle_bar_anim.DORotate(Vector3.zero, 0.2f)
                        .SetEase(Ease.OutElastic);
                });
        }
    }

    private void ShowRewardPanel()
    {
        //set image sprite for reward
     
        rewardImage.sprite = GameDataManager.Instance.GetBoosterDataByID((BoosterID)resultIndex + 1).Icon;
        allItem[resultIndex].Pick();
        StartCoroutine(ShowRewardPanelCoroutine());
    }

    private IEnumerator ShowRewardPanelCoroutine()
    {
        ParticleSystem a = Instantiate(confestyPrefab,transform);
        a.Play();
        float popupDuration = 2f;
        float elapsed = 0f;

        rewardPanel.gameObject.SetActive(true);

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        rewardPanel.gameObject.SetActive(false); 
    }

    private void InitializeSpinWheelItem()
    {
        float angleStep = 360.0f / ItemCount;
        float radius = Mathf.Min(spinWheelTransform.rect.width, spinWheelTransform.rect.height) / 3.5f;
        float scaleRatio = ItemCount > defaultItem ? (float)defaultItem / ItemCount : 1.0f;

        for (int i = 0; i < ItemCount; i++)
        {
            float angle = (i * angleStep + 90) * Mathf.Deg2Rad; // Adjust to start at x = 0
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            SpinItem item = Instantiate(itemPrefab, spinWheelTransform);
            item.transform.localPosition = new Vector3(x, y, 0);
            item.transform.localRotation = Quaternion.Euler(0, 0, 75 * i);
            item.transform.localScale = new Vector3(scaleRatio, scaleRatio, 1);
            item.InitializeItem(
                GameDataManager.Instance.GetBoosterDataByID((BoosterID)i + 1).Icon); //because enum start from 1
            allItem.Add(item);
        }
    }
}
#endif