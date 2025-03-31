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

    private CharacterState currentState;
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
        CharacterDisplay.Instance.OnAngryLevelChanged += OnAngryLevelChanged;
        ScreenInteraction.Instance.OnCharacterInteracted += InitializeCharacterVideo;
    }


    private void OnDestroy()
    {
        CharacterDisplay.Instance.OnCharacterStateChanged -= OnCharacterStateChanged;
        CharacterDisplay.Instance.OnAngryLevelChanged -= OnAngryLevelChanged;
        ScreenInteraction.Instance.OnCharacterInteracted -= InitializeCharacterVideo;
    }

    private void OnAngryLevelChanged(AngryState ang, CharacterState c)
    {
        // else if (c == CharacterState.EngAngry)
        // {
        //     if (ang == AngryState.High)
        //     {
        //         VideoClipInfo angry3End = videoClips.Find(x => x.videoType == VideoType.Angry3End);
        //         InitializeVideoPlayer(angry3End,
        //             CharacterDisplay.Instance.CheckStateAfterVideoPlay);
        //     }
        //     else if (ang == AngryState.Medium)
        //     {
        //         VideoClipInfo angry2End = videoClips.Find(x => x.videoType == VideoType.Angry2End);
        //         InitializeVideoPlayer(angry2End,
        //             CharacterDisplay.Instance.CheckStateAfterVideoPlay);
        //     }
        //     else if (ang == AngryState.Low)
        //     {
        //         VideoClipInfo angry1End = videoClips.Find(x => x.videoType == VideoType.Angry1End);
        //         InitializeVideoPlayer(angry1End,
        //             CharacterDisplay.Instance.CheckStateAfterVideoPlay);
        //     }
        //     else if (ang == AngryState.None)
        //     {
        //         CharacterDisplay.Instance.CheckStateAfterVideoPlay();
        //     }
        // }
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
            else if (clipName.StartsWith("id"))
                type = VideoType.Idle;
            else if (clipName.StartsWith("b"))
                type = VideoType.Thanking;
            else if (clipName.StartsWith("a1"))
                type = VideoType.Angry1;
            else if (clipName.StartsWith("a2"))
                type = VideoType.Angry2;
            else if (clipName.StartsWith("a3"))
                type = VideoType.Angry3;
            else if (clipName.StartsWith("ai1"))
                type = VideoType.AngryIdle1;
            else if (clipName.StartsWith("ai2"))
                type = VideoType.AngryIdle2;
            else if (clipName.StartsWith("ai3"))
                type = VideoType.AngryIdle3;
            else if (clipName.StartsWith("ae1"))
                type = VideoType.Angry1End;
            else if (clipName.StartsWith("ae2"))
                type = VideoType.Angry2End;
            else if (clipName.StartsWith("ae3"))
                type = VideoType.Angry3End;

            videoClips.Add(new VideoClipInfo { videoType = type, videoClip = clip });
        }

        OnLoadVideosComplete?.Invoke(id);
    }


    private void OnCharacterStateChanged(CharacterState state, AngryState ang)
    {
        currentState = state;
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
        else if (state == CharacterState.Entry)
        {
            VideoClipInfo[] greetingVideos = videoClips.FindAll(x => x.videoType == VideoType.Greeting).ToArray();
            VideoClipInfo info = greetingVideos[UnityEngine.Random.Range(0, greetingVideos.Length)];
            InitializeVideoPlayer(info, () =>
            {
                VideoClipInfo[] talkVideos = videoClips.FindAll(x => x.videoType == VideoType.Talking).ToArray();
                VideoClipInfo talkInfo = talkVideos[UnityEngine.Random.Range(0, talkVideos.Length)];
                InitializeVideoPlayer(talkInfo,
                    () => { CharacterDisplay.Instance.TransitionToState(CharacterState.Idle); });
            });
        }
        else if (state == CharacterState.Exit)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
            videoClips.Clear();
        }
        else if (state == CharacterState.Angry)
        {
            if (currentState == CharacterState.Angry)
            {
                if (ang == AngryState.Low)
                {
                    Debug.Log("Angry1");
                    VideoClipInfo idle = videoClips.Find(x => x.videoType == VideoType.Idle);
                    InitializeVideoPlayer(idle, CharacterDisplay.Instance.CheckStateAfterVideoPlay);
                }
                else if (ang == AngryState.Medium)
                {
                    VideoClipInfo angryIdle2 = videoClips.Find(x => x.videoType == VideoType.AngryIdle2);
                    InitializeVideoPlayer(angryIdle2, CharacterDisplay.Instance.CheckStateAfterVideoPlay);
                }
                else
                {
                    VideoClipInfo angryIdle3 = videoClips.Find(x => x.videoType == VideoType.AngryIdle3);
                    InitializeVideoPlayer(angryIdle3, CharacterDisplay.Instance.CheckStateAfterVideoPlay);
                }
            }
            else
            {
                if (ang == AngryState.Low)
                {
                    VideoClipInfo angry1 = videoClips.Find(x => x.videoType == VideoType.Angry1);
                    VideoClipInfo idleInfo1 = videoClips.Find(x => x.videoType == VideoType.AngryIdle1);
                    InitializeVideoPlayer(angry1,
                        () => { InitializeVideoPlayer(idleInfo1, null); });
                }
                else if (ang == AngryState.Medium)
                {
                    VideoClipInfo angry2 = videoClips.Find(x => x.videoType == VideoType.Angry2);
                    VideoClipInfo idleInfo = videoClips.Find(x => x.videoType == VideoType.AngryIdle2);
                    InitializeVideoPlayer(angry2,
                        () => { InitializeVideoPlayer(idleInfo, null); });
                }
                else if (ang == AngryState.High)
                {
                    VideoClipInfo angry3 = videoClips.Find(x => x.videoType == VideoType.Angry3);
                    VideoClipInfo idleInfo = videoClips.Find(x => x.videoType == VideoType.AngryIdle3);
                    InitializeVideoPlayer(angry3,
                        () => { InitializeVideoPlayer(idleInfo, null); });
                }
            }
        }
        else if (state == CharacterState.EngAngry)
        {
            if (ang == AngryState.High)
            {
                VideoClipInfo angry3End = videoClips.Find(x => x.videoType == VideoType.Angry3End);
                InitializeVideoPlayer(angry3End,
                    CharacterDisplay.Instance.CheckStateAfterVideoPlay);
            }
            else if (ang == AngryState.Medium)
            {
                VideoClipInfo angry2End = videoClips.Find(x => x.videoType == VideoType.Angry2End);
                InitializeVideoPlayer(angry2End,
                    CharacterDisplay.Instance.CheckStateAfterVideoPlay);
            }
            else if (ang == AngryState.Low)
            {
                VideoClipInfo angry1End = videoClips.Find(x => x.videoType == VideoType.Angry1End);
                InitializeVideoPlayer(angry1End,
                    CharacterDisplay.Instance.CheckStateAfterVideoPlay);
            }
            else if (ang == AngryState.None)
            {
                CharacterDisplay.Instance.CheckStateAfterVideoPlay();
            }
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
            onEnd?.Invoke();
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
        else if (info.videoType == VideoType.Angry1 || info.videoType == VideoType.Angry2 ||
                 info.videoType == VideoType.Angry3)
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

        else if (info.videoType == VideoType.AngryIdle1 || info.videoType == VideoType.AngryIdle2 ||
                 info.videoType == VideoType.AngryIdle3)
        {
            videoPlayer.isLooping = true;
            videoPlayer.clip = info.videoClip;
            videoPlayer.Play();
            onEnd?.Invoke();
        }
        else if (info.videoType == VideoType.Angry1End || info.videoType == VideoType.Angry2End ||
                 info.videoType == VideoType.Angry3End)
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
    Thanking,
    Angry1,
    Angry2,
    Angry3,
    AngryIdle1,
    AngryIdle2,
    AngryIdle3,
    Angry1End,
    Angry2End,
    Angry3End
}