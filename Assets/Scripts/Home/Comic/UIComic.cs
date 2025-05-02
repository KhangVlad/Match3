using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Add DOTween namespace

public class UIComic : MonoBehaviour
{
    [SerializeField] private Transform comic;
    [SerializeField] private Image frame1;
    [SerializeField] private Image frame2;
    [SerializeField] private Image frame3;
    [SerializeField] private Button skip;

    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayBetweenFrames = 0.5f;
    [SerializeField] private float skipButtonAnimDuration = 0.5f; // Duration for skip button animation

    private void Start()
    {
        SetFrameAlpha(frame1, 0f);
        SetFrameAlpha(frame2, 0f);
        SetFrameAlpha(frame3, 0f);
        
        // Set up skip button
        if (skip != null)
        {
            skip.onClick.AddListener(OnSkipButtonClicked);
            skip.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (skip != null)
        {
            skip.onClick.RemoveListener(OnSkipButtonClicked);
        }
    }

    public void OnNewUserCreate()
    {
        // Start the frame fade-in sequence
        StartCoroutine(FadeInFramesSequentially());
    }

    private void OnSkipButtonClicked()
    {
        StopAllCoroutines();
        SetFrameAlpha(frame1, 1f);
        SetFrameAlpha(frame2, 1f);
        SetFrameAlpha(frame3, 1f);
        StartCoroutine(LoadAfterDelay(0.5f));
    }

    private IEnumerator FadeInFramesSequentially()
    {
        yield return StartCoroutine(FadeInFrame(frame1));
        yield return new WaitForSeconds(delayBetweenFrames);
        
        yield return StartCoroutine(FadeInFrame(frame2));
        yield return new WaitForSeconds(delayBetweenFrames);
        
        yield return StartCoroutine(FadeInFrame(frame3));
        yield return new WaitForSeconds(delayBetweenFrames);
        
        // Animate skip button appearance using DOTween
        AnimateSkipButtonAppearance();
    }

    private void AnimateSkipButtonAppearance()
    {
        if (skip != null)
        {
            skip.gameObject.SetActive(true);
            
            // Reset initial state
            skip.transform.localScale = Vector3.zero;
            
            // Use DOTween to scale the button from 0 to 1
            skip.transform.DOScale(Vector3.one, skipButtonAnimDuration)
                .SetEase(Ease.OutBack) // Add a bouncy effect
                .OnComplete(() => {
                    // Optional: Add a subtle pulsing effect after appearance
                    skip.transform.DOScale(1.05f, 0.5f)
                        .SetLoops(-1, LoopType.Yoyo) // Infinite loop with back-and-forth
                        .SetEase(Ease.InOutSine);
                });
        }
    }

    private IEnumerator FadeInFrame(Image frame)
    {
        float elapsedTime = 0f;
        
        // Start with alpha = 0
        SetFrameAlpha(frame, 0f);
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            SetFrameAlpha(frame, alpha);
            
            yield return null;
        }
        SetFrameAlpha(frame, 1f);
    }

    private void SetFrameAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private IEnumerator LoadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadScene();
    }

    private void LoadScene()
    {
        #if !UNITY_WEBGL
        AuthenticationManager.Instance.HandleChangeScene();
        #endif
    }
}