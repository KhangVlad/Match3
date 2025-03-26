using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class CharacterVideoController : MonoBehaviour
{
    public static CharacterVideoController Instance { get; private set; }
    public List<VideoClipInfo> videoClips = new List<VideoClipInfo>();
    public VideoPlayer videoPlayer;
    public event Action<CharacterID> OnLoadVideosComplete;

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
        CharacterDisplay.Instance.OnCharacterStateChanged += OnCharacterStateChanged;
        CharacterDisplay.Instance.OnCloseDialogue += HandleCloseDialogue;
        ScreenInteraction.Instance.OnCharacterInteracted += InitializeCharacterVideo;
    }


    private void OnDestroy()
    {
        CharacterDisplay.Instance.OnCharacterStateChanged -= OnCharacterStateChanged;
        ScreenInteraction.Instance.OnCharacterInteracted -= InitializeCharacterVideo;
        CharacterDisplay.Instance.OnCloseDialogue -= HandleCloseDialogue;
    }

    private void InitializeCharacterVideo(CharacterID id)
    {
        videoClips.Clear(); // Clear previous clips
        VideoClip[] clips = Resources.LoadAll<VideoClip>($"Characters/{(int)id}");
        if (clips.Length == 0)
        {
            Debug.LogError("No video clips found for character: " + id);
            return;
        }

        foreach (var clip in clips)
        {
            if (clip == null) continue;

            string clipName = clip.name.ToLower();
            VideoType type = VideoType.Idle; // Default to Idle

            if (clipName.StartsWith("g"))
                type = VideoType.Greeting;
            else if (clipName.StartsWith("t"))
                type = VideoType.Talking;
            else if (clipName.StartsWith("i"))
                type = VideoType.Idle;

            videoClips.Add(new VideoClipInfo { videoType = type, videoClip = clip });
        }

        OnLoadVideosComplete?.Invoke(id);
    }


    private void OnCharacterStateChanged(Sympathy s, CharacterState state)
    {
        Debug.Log("OnCharacterStateChanged" + state);
        if (state == CharacterState.Idle)
        {
            Debug.Log("Idle");
            VideoClipInfo info = videoClips.Find(x => x.videoType == VideoType.Idle);
            InitializeVideoPlayer(info, null);
        }
        else if (state == CharacterState.Talking)
        {
            VideoClipInfo[] talkVideos = videoClips.FindAll(x => x.videoType == VideoType.Talking).ToArray();
            VideoClipInfo info = talkVideos[UnityEngine.Random.Range(0, talkVideos.Length)];
            InitializeVideoPlayer(info, () =>
            {
                VideoClipInfo idleInfo = videoClips.Find(x => x.videoType == VideoType.Idle);
                InitializeVideoPlayer(idleInfo, null);
            });
        }
        else if (state == CharacterState.Greeting)
        {
            VideoClipInfo[] greetingVideos = videoClips.FindAll(x => x.videoType == VideoType.Greeting).ToArray();
            VideoClipInfo info = greetingVideos[UnityEngine.Random.Range(0, greetingVideos.Length)];
            InitializeVideoPlayer(info, () =>
            {
                VideoClipInfo idleInfo = videoClips.Find(x => x.videoType == VideoType.Idle);
                InitializeVideoPlayer(idleInfo, null);
            });
        }
    }


    private void InitializeVideoPlayer(VideoClipInfo info, Action onEnd)
    {
        StartCoroutine(PlayVideo(info, onEnd));
    }

    private IEnumerator PlayVideo(VideoClipInfo info, Action onEnd)
    {
        if (info.videoType == VideoType.Idle)
        {
            videoPlayer.isLooping = true;
            videoPlayer.clip = info.videoClip;
            videoPlayer.Play();
        }
        else if (info.videoType == VideoType.Talking || info.videoType == VideoType.Greeting)
        {
            videoPlayer.isLooping = false;
            videoPlayer.clip = info.videoClip;
            videoPlayer.Play();

            // Wait until the video finishes playing
            bool isVideoFinished = false;
            videoPlayer.loopPointReached += source => { isVideoFinished = true; };

            yield return new WaitUntil(() => isVideoFinished);

            onEnd?.Invoke();
        }
    }


    private void HandleCloseDialogue()
    {
        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoClips.Clear();
    }
}

[Serializable]
public class VideoClipInfo
{
    public VideoType videoType;
    public VideoClip videoClip;
}

[Serializable]
public enum VideoType
{
    Idle, //will loop
    Talking, //after playing, will go back to idle
    Greeting, //after playing, will go back to idle
    AfterGreeting //after playing, will go back to idle
}