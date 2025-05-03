using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using Match3.Enums;

namespace Match3
{
    public class GameplayVideo : MonoBehaviour
    {
        #region Singleton Pattern
        public static GameplayVideo Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private List<VideoClipInfo> videoClips = new List<VideoClipInfo>();
        #endregion

        #region Public Properties
        public bool IsPlaying { get; private set; }
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeSingleton();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region Initialization Methods
        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                // Consider DontDestroyOnLoad if this should persist across scenes
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning($"Duplicate GameplayVideo instance found on {gameObject.name}. Destroying...");
                Destroy(gameObject);
            }
        }

        private void SubscribeToEvents()
        {
            if (UIQuestManager.Instance != null)
            {
                UIQuestManager.Instance.OnCollect += OnCollectQuest;
            }
            else
            {
                Debug.LogError("MatchAnimManager.Instance is null in GameplayVideo.Start()");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (UIQuestManager.Instance != null)
            {
                UIQuestManager.Instance.OnCollect -= OnCollectQuest;
            }
        }
        #endregion

        #region Character Video Initialization
        public void InitializeCharacterVideo(CharacterID id)
        {
            videoClips.Clear();
            
            string resourcePath = $"Characters/{(int)id}";
            VideoClip[] clips = Resources.LoadAll<VideoClip>(resourcePath);
            
            if (clips.Length == 0)
            {
                Debug.LogError($"No video clips found for character: {id} at path: {resourcePath}");
                return;
            }

            // Use LINQ for cleaner code and better readability
            videoClips = clips
                .Where(clip => clip != null)
                .Select(clip => new VideoClipInfo 
                { 
                    videoType = DetermineVideoType(clip.name), 
                    videoClip = clip 
                })
                .ToList();

            Debug.Log($"Loaded {videoClips.Count} video clips for character {id}");
        }

        private VideoType DetermineVideoType(string clipName)
        {
            string lowerClipName = clipName.ToLower();
            
            // Dictionary for easier mapping and extensibility
            var nameToType = new Dictionary<string, VideoType>
            {
                { "g", VideoType.Greeting },
                { "idle", VideoType.Idle },
                { "angry", VideoType.Angry3Idle },
                { "success", VideoType.Success }
            };

            foreach (var pair in nameToType)
            {
                if (lowerClipName.StartsWith(pair.Key))
                {
                    return pair.Value;
                }
            }

            // Default to Idle if no match is found
            Debug.LogWarning($"Unknown video clip type for: {clipName}. Defaulting to Idle.");
            return VideoType.Idle;
        }
        #endregion

        #region Video Playback Methods
        private void OnCollectQuest()
        {
            if (IsPlaying)
            {
                Debug.Log("Video is already playing. Ignoring OnCollectQuest.");
                return;
            }

            VideoClipInfo successClip = FindVideoClip(VideoType.Success);
            if (successClip != null)
            {
                PlayVideo(successClip, () => IsPlaying = false);
            }
            else
            {
                Debug.LogWarning("No Success video clip found.");
            }
        }

        public void PlayVideo(VideoType type, Action onComplete = null)
        {
            VideoClipInfo clipInfo = FindVideoClip(type);
            if (clipInfo != null)
            {
                PlayVideo(clipInfo, onComplete);
            }
            else
            {
                Debug.LogWarning($"No video clip found for type: {type}");
                onComplete?.Invoke();
            }
        }

        private void PlayVideo(VideoClipInfo clipInfo, Action onComplete, bool skipWait = false)
        {
            StartCoroutine(PlayVideoCoroutine(clipInfo, onComplete, skipWait));
        }

        private IEnumerator PlayVideoCoroutine(VideoClipInfo clipInfo, Action onComplete, bool skipWait)
        {
            if (clipInfo?.videoClip == null)
            {
                Debug.LogError($"Invalid video clip info for type: {clipInfo?.videoType}");
                onComplete?.Invoke();
                yield break;
            }

            IsPlaying = true;

            try
            {
                ConfigureVideoPlayer(clipInfo);
                videoPlayer.Play();

                // Wait for video to finish if not looping and not skipping wait
                if (!skipWait && !videoPlayer.isLooping)
                {
                    yield return new WaitForSeconds((float)clipInfo.videoClip.length);
                }
            }
            finally
            {
                IsPlaying = false;
                onComplete?.Invoke();
            }
        }

        private void ConfigureVideoPlayer(VideoClipInfo clipInfo)
        {
            videoPlayer.clip = clipInfo.videoClip;
            videoPlayer.isLooping = clipInfo.videoType == VideoType.Idle;
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
        }
        #endregion

        #region Helper Methods
        private VideoClipInfo FindVideoClip(VideoType type)
        {
            return videoClips.FirstOrDefault(clip => clip.videoType == type);
        }
        #endregion
    }
}