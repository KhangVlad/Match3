using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIComic : MonoBehaviour
{
    [SerializeField] private Transform comic;
    [SerializeField] private Image frame1;
    [SerializeField] private Image frame2;
    [SerializeField] private Image frame3;
    [SerializeField] private Button skip;

    [SerializeField] private float fadeDuration = 1f;
    
    private int currentFrameIndex = 0;
    private Image[] frames;
    private bool isAnimating = false;

    private void Start()
    {
        
        // Initialize frames array
        frames = new Image[] { frame1, frame2, frame3 };
        
        // Initially hide all frames
        foreach (var frame in frames)
        {
            SetFrameAlpha(frame, 0f);
        }
        
        // Set up skip button
        if (skip != null)
        {
            skip.onClick.AddListener(OnSkipButtonClicked);
        }
        
        // Initially hide the comic
        if (comic != null)
        {
            comic.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        
        // Unsubscribe from skip button
        if (skip != null)
        {
            skip.onClick.RemoveListener(OnSkipButtonClicked);
        }
    }

    public void OnNewUserCreate()
    {
        if (comic != null)
        {
            comic.gameObject.SetActive(true);
        }
        
        // Reset to first frame
        currentFrameIndex = 0;
        
        // Start fading in the first frame
        StartCoroutine(FadeInCurrentFrame());
    }

    private void OnSkipButtonClicked()
    {
        // If animation is in progress, wait for it to finish
        if (isAnimating)
            return;
            
        // Advance to the next frame
        AdvanceToNextFrame();
    }
    
    private void AdvanceToNextFrame()
    {
        // Move to next frame index
        currentFrameIndex++;
        
        // If we've shown all frames, load the next scene
        if (currentFrameIndex >= frames.Length)
        {
            LoadScene();
            return;
        }
        
        // Otherwise, fade in the next frame
        StartCoroutine(FadeInCurrentFrame());
    }

    private IEnumerator FadeInCurrentFrame()
    {
        isAnimating = true;
        
        // Get the current frame
        Image currentFrame = frames[currentFrameIndex];
        
        float elapsedTime = 0f;
        SetFrameAlpha(currentFrame, 0f);
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            SetFrameAlpha(currentFrame, alpha);
            
            yield return null;
        }
        
        // Ensure the frame is fully visible
        SetFrameAlpha(currentFrame, 1f);
        
        isAnimating = false;
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

    private void LoadScene()
    {
        AuthenticationManager.Instance.HandleChangeScene();
    }
}