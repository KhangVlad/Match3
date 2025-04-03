using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Video;
using System.Collections;

public class CharacterDisplay : MonoBehaviour
{
    public static CharacterDisplay Instance { get; private set; }
    public CharacterDialogueSO characterDialogueSO;

    public bool IsActiveCharacter = false;

    // public event Action<CharacterState, AngryState> OnCharacterStateChanged;
    public event Action<AngryState, bool> OnAngryLevelChanged; //bool is state change by increase by time or on touch

    public CharacterState state;
    public AngryState angryState;
    public float AngryPoint = 0;
    public bool IsAngry => AngryPoint > 0;
    private readonly int[] AngryThreshold = { 1, 11, 30, 50 };
    private const float AngryDecayRate = 2f;
    private const string rejectDialogue = "This quest is too hard for you, I will ask someone else.";

    private const string notEnoughSympathyDialogue =
        "I don't think you are the right person to help me now, but i will ask you later.";

    public float TimeToDecreaseAngryPoint = 10; //after 10s not touch, decrease angry point
    private float lastInteractionTime; // Track last interaction time

    public bool IsRecovering;

    public string GetDialogue()
    {
        int randomIndex = UnityEngine.Random.Range(0, characterDialogueSO.greetingDialogs.Length);
        return characterDialogueSO.greetingDialogs[randomIndex];
    }

    public string GetDialogue(int level, int subLevel)
    {
        return characterDialogueSO.data[level].levelDialogs[subLevel];
    }

    public string GetNotEnoughSympathyDialogue() => notEnoughSympathyDialogue;

    public string GetRejectDialogue() => rejectDialogue;


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
        lastInteractionTime = Time.time;
        ScreenInteraction.Instance.OnCharacterInteracted += LoadCharacterDialogue;
        ScreenInteraction.Instance.OnCharacterInteracted += InitializeCharacterVideo;
    }

    private void OnDestroy()
    {
        ScreenInteraction.Instance.OnCharacterInteracted -= LoadCharacterDialogue;
        ScreenInteraction.Instance.OnCharacterInteracted -= InitializeCharacterVideo;
    }


    #region State

    private void LoadCharacterDialogue(CharacterID id)
    {
        characterDialogueSO = CharactersDataManager.Instance.GetCharacterDialogue(id);
    }

    public void TransitionToState(CharacterState newState)
    {
        state = newState;
        IsActiveCharacter = (state != CharacterState.Exit);
        OnCharacterStateChanged(state, angryState);
    }

    private void Update()
    {
        // if (!IsActiveCharacter || state == CharacterState.Entry) return;
        // HandleAngryState();

        if (!IsActiveCharacter || state == CharacterState.Entry) return;
        HandleAngryState();

        // Check if it's time to start decreasing anger
        if (Time.time - lastInteractionTime > TimeToDecreaseAngryPoint)
        {
            IsRecovering = true;
            DecreaseAnger(Time.deltaTime * AngryDecayRate);
        }
    }


    private void HandleAngryState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IncreaseAnger(5);
            IsRecovering = false;
            lastInteractionTime = Time.time; // Reset interaction timer
        }

        if (IsAngry && IsRecovering)
        {
            AngryState previousState = angryState;
            if (previousState != GetAngryStateFromPoint(AngryPoint))
            {
                angryState = GetAngryStateFromPoint(AngryPoint);
                state = CharacterState.EngAngry;
                TransitionToState(CharacterState.EngAngry);
            }
        }
    }

    private void PlayCurrentState()
    {
        if (state == CharacterState.Angry && angryState == AngryState.None)
        {
            TransitionToState(CharacterState.Idle);
        }
        else if (state == CharacterState.EngAngry && angryState == AngryState.None)
        {
            TransitionToState(CharacterState.Idle);
        }
        else if (state == CharacterState.Exit)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
            videoClips.Clear();
        }
    }


    private void IncreaseAnger(float amount)
    {
        AngryPoint += amount;
        AngryState previousState = angryState;

        if (previousState != GetAngryStateFromPoint(AngryPoint))
        {
            angryState = GetAngryStateFromPoint(AngryPoint);
            state = CharacterState.Angry;
            TransitionToState(CharacterState.Angry);
        }


        // Clamp the value to the maximum threshold
    }

    private void DecreaseAnger(float amount)
    {
        AngryPoint = Mathf.Max(0, AngryPoint - amount);
        if (AngryPoint <= 0)
        {
            IsRecovering = false;
        }
    }


    private AngryState GetAngryStateFromPoint(float point)
    {
        if (point >= AngryThreshold[3]) return AngryState.High;
        if (point >= AngryThreshold[2]) return AngryState.Medium;
        if (point >= AngryThreshold[1]) return AngryState.Low;
        if (point >= AngryThreshold[0]) return AngryState.CompletelyRecover;

        return AngryState.None;
    }

    #endregion


    #region VideoPlay

    public List<VideoClipInfo> videoClips = new List<VideoClipInfo>();
    public VideoPlayer videoPlayer;


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
            InitializeVideoPlayer(info, () => { TransitionToState(CharacterState.Idle); });
        }

        else if (state == CharacterState.Angry)
        {
            if (IsRecovering)
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
                    TransitionToState(CharacterState.Idle);
                }
            }
            else
            {
                Debug.Log("Angry by touch" + ang);
                if (ang == AngryState.High)
                {
                    Debug.Log("Angry3Start");
                    VideoClipInfo a3 = videoClips.Find(x => x.videoType == VideoType.Angry3Start);
                    InitializeVideoPlayer(a3, null, true);
                }
                else if (ang == AngryState.Medium)
                {
                    Debug.Log("Angry2Start");
                    VideoClipInfo a2 = videoClips.Find(x => x.videoType == VideoType.Angry2Start);
                    InitializeVideoPlayer(a2, null, true);
                }
                else if (ang == AngryState.Low)
                {
                    VideoClipInfo a1 = videoClips.Find(x => x.videoType == VideoType.Angry1Start);
                    if (a1 != null)
                    {
                        Debug.Log("Angry1Start");
                        InitializeVideoPlayer(a1, null, true);
                    }
                    else
                    {
                        Debug.LogError("Angry1Start video clip not found in the list.");
                    }
                }
            }
        }
        else if (state == CharacterState.EngAngry)
        {
            // AngryState previousAngryState = ang + 1;
            if (ang == AngryState.High)
            {
                VideoClipInfo ae3 = videoClips.Find(x => x.videoType == VideoType.Angry3End);
                VideoClipInfo id2 = videoClips.Find(x => x.videoType == VideoType.Angry2Idle);
                InitializeVideoPlayer(ae3, () => InitializeVideoPlayer(id2, PlayCurrentState));
            }
            else if (ang == AngryState.Medium)
            {
                VideoClipInfo ae2 = videoClips.Find(x => x.videoType == VideoType.Angry2End);
                VideoClipInfo id1 = videoClips.Find(x => x.videoType == VideoType.Angry1Idle);
                InitializeVideoPlayer(ae2, () => InitializeVideoPlayer(id1, PlayCurrentState));
            }
            else if (ang == AngryState.Low)
            {
                VideoClipInfo ae1 = videoClips.Find(x => x.videoType == VideoType.Angry1End);
                VideoClipInfo id = videoClips.Find(x => x.videoType == VideoType.Idle);
                InitializeVideoPlayer(ae1, () => InitializeVideoPlayer(id, PlayCurrentState));
            }
            else if (ang == AngryState.CompletelyRecover)
            {
                VideoClipInfo ae1 = videoClips.Find(x => x.videoType == VideoType.Angry1End);
                VideoClipInfo id = videoClips.Find(x => x.videoType == VideoType.Idle);
                InitializeVideoPlayer(ae1, () => InitializeVideoPlayer(id, PlayCurrentState));
            }
        }
        else if (state == CharacterState.Exit)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
            videoClips.Clear();
        }
    }


    private void InitializeVideoPlayer(VideoClipInfo info, Action onEnd, bool skipWait = false)
    {
        StartCoroutine(PlayVideo(info, onEnd, skipWait));
    }

    private IEnumerator PlayVideo(VideoClipInfo info, Action onEnd, bool skipWait)
    {
        if (info == null)
        {
            Debug.Log("Video clip is null for type: ");
            yield break; // Exit the coroutine if the video clip is null
        }

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

    #endregion
}

public enum CharacterState
{
    Idle,
    Talking,
    Greeting,
    Entry,
    Exit,
    Angry,
    EngAngry,
}

public enum AngryState
{
    None = 100,
    CompletelyRecover = 0,
    Low = 1,
    Medium = 2,
    High = 3,
}