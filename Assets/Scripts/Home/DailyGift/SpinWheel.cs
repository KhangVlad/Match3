// using System;
// using UnityEngine;
// using System.Collections;
// using Match3;
// using TMPro;
// using UnityEngine.UI;
// using Firebase.Firestore;
//
// #if !UNITY_WEBGL
// public class SpinWheel : MonoBehaviour
// {
//     public AnimationCurve AnimationCurve;
//     [SerializeField] private RectTransform spinWheelTransform;
//     [SerializeField] private Button spinButton;
//     [SerializeField] private Animator handle_bar_anim;
//     private const string SHAKE = "Shake"; //TRIGGER
//     private int defaultItem = 5;
//     public SpinItem itemPrefab;
//
//     public int ItemCount = 5; //360/8 add to spinWheelTransform ,adjust x and y
//
//     private bool m_spinning = false;
//     private int selectedItemIndex = -1;
//     private float lastAngle = 0f;
//
//     public int resultIndex; //spin wheel will return this index
//
//     [SerializeField] private Transform rewardPanel;
//     [SerializeField] private Image rewardImage;
//     [SerializeField] private TextMeshProUGUI timeNextSpin;
//     private DateTime LastOnlineTime;
//     
//     
//     private void Start()
//     {
//         spinButton.onClick.AddListener(Spin);
//         InitializeSpinWheelItem();
//         LastOnlineTime = TimeManager.Instance.LastOnlineTime;
//     }
//
//     private void OnDestroy()
//     {
//         spinButton.onClick.RemoveListener(Spin);
//     }
//
//
//     private void Update()
//     {
//         
//     }
//
//     public void Spin()
//     {
//         UserManager.Instance.UserData.LastSpinTime = FieldValue.ServerTimestamp;
//         UserManager.Instance.ClaimDailyReward(BoosterID.Hammer, 1);
//         if (!m_spinning)
//             StartCoroutine(DoSpin());
//     }
//
//     private IEnumerator DoSpin()
//     {
//         m_spinning = true;
//         spinButton.interactable = false;
//
//         // Select the result index
//         // resultIndex = Random.Range(0, ItemCount); // Ensure resultIndex is set correctly
//         float angleStep = 360f / ItemCount;
//         float extraRotations = 3; // Adds extra rotations before stopping
//         float targetAngle = -(resultIndex * angleStep + (extraRotations * 360));
//
//         float duration = 2.0f; // Spin duration
//         float elapsed = 0f;
//         float startAngle = spinWheelTransform.eulerAngles.z;
//         lastAngle = startAngle;
//
//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / duration;
//             float curveValue = AnimationCurve.Evaluate(t); // Smooth curve effect
//             float newAngle = Mathf.Lerp(startAngle, targetAngle, curveValue);
//             spinWheelTransform.eulerAngles = new Vector3(0, 0, newAngle);
//
//             // Play shake animation when crossing sections
//             CheckAngleChangeForShake(newAngle);
//
//             yield return null;
//         }
//
//         // Snap to final result exactly
//         spinWheelTransform.eulerAngles = new Vector3(0, 0, targetAngle % 360);
//
//         // No final shake animation at the result
//
//         // Debug booster id
//         Debug.Log($"{(BoosterID)resultIndex + 1}");
//         m_spinning = false;
//
//         ShowRewardPanel();
//     }
//
//     private void CheckAngleChangeForShake(float newAngle)
//     {
//         // Normalize angles to 0-360 range
//         float normalizedLastAngle = ((lastAngle % 360) + 360) % 360;
//         float normalizedNewAngle = ((newAngle % 360) + 360) % 360;
//
//         // Calculate the angle step based on item count
//         float angleStep = 360f / ItemCount;
//
//         // Check if we've crossed a section boundary
//         int lastSection = Mathf.FloorToInt(normalizedLastAngle / angleStep);
//         int newSection = Mathf.FloorToInt(normalizedNewAngle / angleStep);
//
//         if (lastSection != newSection)
//         {
//             PlayShakeAnimation();
//         }
//
//         lastAngle = newAngle;
//     }
//
//     private void PlayShakeAnimation()
//     {
//         if (handle_bar_anim != null)
//         {
//             handle_bar_anim.SetTrigger(SHAKE);
//         }
//     }
//
//     private void ShowRewardPanel()
//     {
//         //set image sprite for reward
//         rewardImage.sprite = GameDataManager.Instance.GetBoosterDataByID((BoosterID)resultIndex + 1).Icon;
//         StartCoroutine(ShowRewardPanelCoroutine());
//     }
//
//     private IEnumerator ShowRewardPanelCoroutine()
//     {
//         float popupDuration = 2f;
//         float elapsed = 0f;
//
//         rewardPanel.gameObject.SetActive(true); // Ensure the panel is active
//
//         while (elapsed < popupDuration)
//         {
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
//
//         rewardPanel.gameObject.SetActive(false); // Hide the panel after showing
//         spinButton.interactable = true;
//     }
//
//
//     private void InitializeSpinWheelItem()
//     {
//         float angleStep = 360.0f / ItemCount;
//         float radius = Mathf.Min(spinWheelTransform.rect.width, spinWheelTransform.rect.height) / 3.5f;
//         float scaleRatio = ItemCount > defaultItem ? (float)defaultItem / ItemCount : 1.0f;
//
//         for (int i = 0; i < ItemCount; i++)
//         {
//             float angle = (i * angleStep + 90) * Mathf.Deg2Rad; // Adjust to start at x = 0
//             float x = Mathf.Cos(angle) * radius;
//             float y = Mathf.Sin(angle) * radius;
//             SpinItem item = Instantiate(itemPrefab, spinWheelTransform);
//             item.transform.localPosition = new Vector3(x, y, 0);
//             item.transform.localRotation = Quaternion.Euler(0, 0, 75 * i);
//             item.transform.localScale = new Vector3(scaleRatio, scaleRatio, 1);
//             item.InitializeItem(
//                 GameDataManager.Instance.GetBoosterDataByID((BoosterID)i + 1)); //becau enum start from 1
//         }
//     }
// }
// #endif

using System;
using UnityEngine;
using System.Collections;
using Match3;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;

#if !UNITY_WEBGL
public class SpinWheel : MonoBehaviour
{
    public AnimationCurve AnimationCurve;
    [SerializeField] private RectTransform spinWheelTransform;
    [SerializeField] private Button spinButton;
    [SerializeField] private Animator handle_bar_anim;
    private const string SHAKE = "Shake"; //TRIGGER
    private int defaultItem = 5;
    public SpinItem itemPrefab;

    public int ItemCount = 5; //360/8 add to spinWheelTransform ,adjust x and y

    private bool m_spinning = false;
    private int selectedItemIndex = -1;
    private float lastAngle = 0f;

    public int resultIndex; //spin wheel will return this index

    [SerializeField] private Transform rewardPanel;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI timeNextSpin;
    private DateTime lastSpinTime;

    // Added variables for spin cooldown
    private const int HOURS_BETWEEN_SPINS = 12;
    private bool canSpin = false;

    private void Start()
    {
        spinButton.onClick.AddListener(Spin);   
        InitializeSpinWheelItem();
        lastSpinTime = TimeManager.Instance.LastSpinTime;

        // Check if player can spin on start
        CheckSpinAvailability();
    }

    private void OnDestroy()
    {
        spinButton.onClick.RemoveListener(Spin);
    }

    private void Update()
    {
        // Update timer text
        UpdateNextSpinText();
    }

    private void CheckSpinAvailability()
    {
        TimeSpan timeSinceLastSpin = DateTime.Now - (DateTime)UserManager.Instance.UserData.LastSpinTime;
        canSpin = timeSinceLastSpin.TotalHours >= HOURS_BETWEEN_SPINS;
        spinButton.interactable = canSpin && !m_spinning;
    }

    private void UpdateNextSpinText()
    {
        if (canSpin)
        {
            // If player can spin, show "Spin Now!" text
            timeNextSpin.text = "Spin Now!";
            return;
        }

        // Calculate remaining time
        TimeSpan timeSinceLastSpin = DateTime.Now - lastSpinTime;
        TimeSpan timeRemaining = TimeSpan.FromHours(HOURS_BETWEEN_SPINS) - timeSinceLastSpin;

        // If time remaining is negative, player can spin
        if (timeRemaining.TotalSeconds <= 0)
        {
            canSpin = true;
            spinButton.interactable = true;
            timeNextSpin.text = "Spin Now!";
            return;
        }

        // Format and display remaining time
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
            timeRemaining.Hours,
            timeRemaining.Minutes,
            timeRemaining.Seconds);
        timeNextSpin.text = $"spin in: {formattedTime}";
    }

    public void Spin()
    {
        // Check if player can spin
        if (!canSpin)
        {
            Debug.Log("Cannot spin yet. Wait for cooldown to finish.");
            return;
        }

        UserManager.Instance.UserData.LastSpinTime = FieldValue.ServerTimestamp;
        UserManager.Instance.ClaimDailyReward(BoosterID.Hammer, 1);

        // Update last online time to current time when spinning
        lastSpinTime = DateTime.Now;

        // Reset spin availability
        canSpin = false;

        if (!m_spinning)
            StartCoroutine(DoSpin());
    }

    private IEnumerator DoSpin()
    {
        m_spinning = true;
        spinButton.interactable = false;

        // Select the result index
        // resultIndex = Random.Range(0, ItemCount); // Ensure resultIndex is set correctly
        float angleStep = 360f / ItemCount;
        float extraRotations = 3; // Adds extra rotations before stopping
        float targetAngle = -(resultIndex * angleStep + (extraRotations * 360));

        float duration = 2.0f; // Spin duration
        float elapsed = 0f;
        float startAngle = spinWheelTransform.eulerAngles.z;
        lastAngle = startAngle;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = AnimationCurve.Evaluate(t); // Smooth curve effect
            float newAngle = Mathf.Lerp(startAngle, targetAngle, curveValue);
            spinWheelTransform.eulerAngles = new Vector3(0, 0, newAngle);

            // Play shake animation when crossing sections
            CheckAngleChangeForShake(newAngle);

            yield return null;
        }

        // Snap to final result exactly
        spinWheelTransform.eulerAngles = new Vector3(0, 0, targetAngle % 360);

        // No final shake animation at the result

        // Debug booster id
        Debug.Log($"{(BoosterID)resultIndex + 1}");
        m_spinning = false;

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
            handle_bar_anim.SetTrigger(SHAKE);
        }
    }

    private void ShowRewardPanel()
    {
        //set image sprite for reward
        rewardImage.sprite = GameDataManager.Instance.GetBoosterDataByID((BoosterID)resultIndex + 1).Icon;
        StartCoroutine(ShowRewardPanelCoroutine());
    }

    private IEnumerator ShowRewardPanelCoroutine()
    {
        float popupDuration = 2f;
        float elapsed = 0f;

        rewardPanel.gameObject.SetActive(true); // Ensure the panel is active

        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        rewardPanel.gameObject.SetActive(false); // Hide the panel after showing

        // Check spin availability again after reward is shown
        // Don't enable the button here, let the CheckSpinAvailability method handle it
        CheckSpinAvailability();
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
                GameDataManager.Instance.GetBoosterDataByID((BoosterID)i + 1)); //becau enum start from 1
        }
    }
}
#endif