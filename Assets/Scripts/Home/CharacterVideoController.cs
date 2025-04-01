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
        ScreenInteraction.Instance.OnCharacterInteracted += InitializeCharacterVideo;
    }


    private void OnDestroy()
    {
        CharacterDisplay.Instance.OnCharacterStateChanged -= OnCharacterStateChanged;
        ScreenInteraction.Instance.OnCharacterInteracted -= InitializeCharacterVideo;
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
            else if (clipName.StartsWith("talk"))
                type = VideoType.Talking;
            else if (clipName.StartsWith("idle"))
                type = VideoType.Idle;
            else if (clipName.StartsWith("thanks"))
                type = VideoType.Thanking;
            else if (clipName.StartsWith("angry_1_start"))
                type = VideoType.Angry1Start;
            else if (clipName.StartsWith("angry_2_start"))
                type = VideoType.Angry2Start;
            else if (clipName.StartsWith("angry_3_start"))
                type = VideoType.Angry3Start;
            else if (clipName.StartsWith("angry_1_end"))
                type = VideoType.Angry1End;
            else if (clipName.StartsWith("angry_2_end"))
                type = VideoType.Angry2End;
            else if (clipName.StartsWith("angry_3_end"))
                type = VideoType.Angry3End;
            else if (clipName.StartsWith("angry_1_idle"))
                type = VideoType.Angry1Idle;
            else if (clipName.StartsWith("angry_2_idle"))
                type = VideoType.Angry2Idle;
            else if (clipName.StartsWith("angry_3_idle"))
                type = VideoType.Angry3Idle;


            videoClips.Add(new VideoClipInfo { videoType = type, videoClip = clip });
        }

        OnLoadVideosComplete?.Invoke(id);
    }


    private void OnCharacterStateChanged(CharacterState state, AngryState ang)
    {
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
                // VideoClipInfo[] talkVideos = videoClips.FindAll(x => x.videoType == VideoType.Talking).ToArray();
                // VideoClipInfo talkInfo = talkVideos[UnityEngine.Random.Range(0, talkVideos.Length)];
                // InitializeVideoPlayer(talkInfo,
                //     () => { CharacterDisplay.Instance.TransitionToState(CharacterState.Idle); });
                CharacterDisplay.Instance.TransitionToState(CharacterState.Idle);
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
            if (CharacterDisplay.Instance.IsRecovering)
            {
                Debug.Log("Recovering");
                if (ang == AngryState.High)
                {
                    VideoClipInfo idle3 = videoClips.Find(x => x.videoType == VideoType.Angry3Idle);
                    InitializeVideoPlayer(idle3, null);
                }
                else if (ang == AngryState.Medium)
                {
                    VideoClipInfo idle2 = videoClips.Find(x => x.videoType == VideoType.Angry2Idle);
                    InitializeVideoPlayer(idle2, null);
                }
                else if (ang == AngryState.Low)
                {
                    VideoClipInfo idle1 = videoClips.Find(x => x.videoType == VideoType.Angry1Idle);
                    InitializeVideoPlayer(idle1, null);
                }
                else if (ang == AngryState.None)
                {
                    CharacterDisplay.Instance.TransitionToState(CharacterState.Idle);
                }
            }
            else
            {
                if (ang == AngryState.High)
                {
                    VideoClipInfo a3 = videoClips.Find(x => x.videoType == VideoType.Angry3Start);
                    InitializeVideoPlayer(a3, null, true);
                }
                else if (ang == AngryState.Medium)
                {
                    VideoClipInfo a2 = videoClips.Find(x => x.videoType == VideoType.Angry2Start);
                    InitializeVideoPlayer(a2, null, true);
                }
                else if (ang == AngryState.Low)
                {
                    VideoClipInfo a1 = videoClips.Find(x => x.videoType == VideoType.Angry1Start);
                    InitializeVideoPlayer(a1, null, true);
                }
            }
        }

        else if (state == CharacterState.EngAngry)
        {
            Debug.Log("ang" + ang);
            AngryState previousAngryState = ang + 1;
            Debug.Log("prev" + previousAngryState);
            if (previousAngryState == AngryState.High)
            {
                VideoClipInfo ae3 = videoClips.Find(x => x.videoType == VideoType.Angry3End);
                VideoClipInfo id2 = videoClips.Find(x => x.videoType == VideoType.Angry1Idle);
                InitializeVideoPlayer(ae3,
                    () => InitializeVideoPlayer(id2, CharacterDisplay.Instance.PlayCurrentState));
            }
            else if (previousAngryState == AngryState.Medium)
            {
                VideoClipInfo ae2 = videoClips.Find(x => x.videoType == VideoType.Angry2End);
                VideoClipInfo id1 = videoClips.Find(x => x.videoType == VideoType.Angry1Idle);
                InitializeVideoPlayer(ae2,
                    () => InitializeVideoPlayer(id1, CharacterDisplay.Instance.PlayCurrentState));
            }
            else if (previousAngryState == AngryState.Low)
            {
                VideoClipInfo ae1 = videoClips.Find(x => x.videoType == VideoType.Angry1End);
                VideoClipInfo id = videoClips.Find(x => x.videoType == VideoType.Idle);
                InitializeVideoPlayer(ae1, () => InitializeVideoPlayer(id, CharacterDisplay.Instance.PlayCurrentState));
            }
            else if (previousAngryState == AngryState.None)
            {
                CharacterDisplay.Instance.TransitionToState(CharacterState.Idle);
            }
        }
    }


    private void InitializeVideoPlayer(VideoClipInfo info, Action onEnd, bool skipWait = false)
    {
        StartCoroutine(PlayVideo(info, onEnd, skipWait));
    }

    private IEnumerator PlayVideo(VideoClipInfo info, Action onEnd, bool skipWait)
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
        else if (info.videoType == VideoType.Angry1Start || info.videoType == VideoType.Angry2Start ||
                 info.videoType == VideoType.Angry3Start)
        {
            videoPlayer.isLooping = false;
            videoPlayer.clip = info.videoClip;
            videoPlayer.Play();
            bool isVideoFinished = false;
            videoPlayer.loopPointReached += source => { isVideoFinished = true; };
            if (skipWait) yield break;
            yield return new WaitUntil(() => isVideoFinished);
            onEnd?.Invoke();
        }

        else if (info.videoType == VideoType.Angry1Idle || info.videoType == VideoType.Angry2Idle ||
                 info.videoType == VideoType.Angry3Idle)
        {
            videoPlayer.isLooping = true;
            videoPlayer.clip = info.videoClip;
            videoPlayer.Play();
            bool isVideoFinished = false;
            videoPlayer.loopPointReached += source => { isVideoFinished = true; };
            yield return new WaitUntil(() => isVideoFinished);
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
    Angry1Start,
    Angry2Start,
    Angry3Start,
    Angry1Idle,
    Angry2Idle,
    Angry3Idle,
    Angry1End,
    Angry2End,
    Angry3End,
}