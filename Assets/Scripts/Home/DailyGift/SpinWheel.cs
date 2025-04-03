using UnityEngine;
using System.Collections;
using Match3;
using UnityEngine.UI;

public class SpinWheel : MonoBehaviour
{
    public AnimationCurve AnimationCurve;
    [SerializeField] private RectTransform spinWheelTransform;
    [SerializeField] private Button spinButton;
    private int defaultItem = 5;
    public SpinItem itemPrefab;

    public int ItemCount = 5; //360/8 add to spinWheelTransform ,adjust x and y

    private bool m_spinning = false;
    private int selectedItemIndex = -1;

    public int resultIndex; //spin wheel will return this index

    [SerializeField] private Transform rewardPanel;
    [SerializeField] private Image rewardImage;

    public void Spin()
    {
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

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = AnimationCurve.Evaluate(t); // Smooth curve effect
            float newAngle = Mathf.Lerp(startAngle, targetAngle, curveValue);
            spinWheelTransform.eulerAngles = new Vector3(0, 0, newAngle);
            yield return null;
        }

        // Snap to final result exactly
        spinWheelTransform.eulerAngles = new Vector3(0, 0, targetAngle % 360);

        // Debug booster id
        Debug.Log($"{(BoosterID)resultIndex + 1}");
        m_spinning = false;

        ShowRewardPanel();
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
        spinButton.interactable = true;
    }

    private void Start()
    {
        spinButton.onClick.AddListener(Spin);
        InitializeSpinWheelItem();
    }

    private void OnDestroy()
    {
        spinButton.onClick.RemoveListener(Spin);
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
            item.transform.localRotation = Quaternion.Euler(0, 0, -i * angleStep);
            item.transform.localScale = new Vector3(scaleRatio, scaleRatio, 1);
            item.InitializeItem(
                GameDataManager.Instance.GetBoosterDataByID((BoosterID)i + 1)); //becau enum start from 1
        }
    }
}