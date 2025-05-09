#if !UNITY_WEBGL
using System;
using UnityEngine;
using System.Collections.Generic;
using Firebase.Firestore;
using Match3.Enums;
using Match3.Shares;
using System.Threading.Tasks;

    [Serializable]
    public class SerializableDateTime
    {
        [SerializeField] private int year;
        [SerializeField] private int month;
        [SerializeField] private int day;
        [SerializeField] private int hour;
        [SerializeField] private int minute;
        [SerializeField] private int second;

        public DateTime DateTime
        {
            get => new DateTime(year, month, day, hour, minute, second);
            set
            {
                year = value.Year;
                month = value.Month;
                day = value.Day;
                hour = value.Hour;
                minute = value.Minute;
                second = value.Second;
            }
        }

        public SerializableDateTime()
        {
            DateTime = DateTime.Now;
        }

        public SerializableDateTime(DateTime dateTime)
        {
            DateTime = dateTime;
        }
    }

    public class TimeManager : MonoBehaviour
    {
        #region Singleton
        public static TimeManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Constants
        private const float DEFAULT_DISPATCH_INTERVAL = 60f;
        private const int DEFAULT_MAX_ENERGY = 25;
        #endregion

        #region SerializedFields
        [Header("Time Settings")]
        [SerializeField] private int maxEnergy = DEFAULT_MAX_ENERGY;
        [SerializeField] private float dispatchInterval = DEFAULT_DISPATCH_INTERVAL;
        
        [Header("Time Tracking")]
        [SerializeField] private SerializableDateTime serverTime;
        [SerializeField] private SerializableDateTime lastOnlineTime = new();
        [SerializeField] private SerializableDateTime loginTime = new();
        #endregion

        #region Private Variables
        private int currentHour;
        private TimeOfDay currentTimeOfDay;
        public float minuteCounter = 0f;
        private double serverTimeOffset = 0;
        #endregion

        #region Public Properties

        public bool IsTimeValid;

    
        public DateTime LastOnlineTime
        {
            get => lastOnlineTime.DateTime;
            set => lastOnlineTime.DateTime = value;
        }

        public DateTime LoginTime
        {
            get => loginTime.DateTime;
            set => loginTime.DateTime = value;
        }

        public DateTime ServerTime
        {
            get
            {
                if (!IsTimeValid)
                    return DateTime.UtcNow;
                return serverTime.DateTime.AddSeconds(serverTimeOffset);
            }
        }
        #endregion

        #region Events
        public event Action OnTimeChanged;
        public event Action OnMinuteElapsed;
        #endregion

        #region Time Mappings
        private static readonly Dictionary<DayOfWeek, DayInWeek> DayMapping = new()
        {
            { DayOfWeek.Monday, DayInWeek.Monday },
            { DayOfWeek.Tuesday, DayInWeek.Tuesday },
            { DayOfWeek.Wednesday, DayInWeek.Wednesday },
            { DayOfWeek.Thursday, DayInWeek.Thursday },
            { DayOfWeek.Friday, DayInWeek.Friday },
            { DayOfWeek.Saturday, DayInWeek.Saturday },
            { DayOfWeek.Sunday, DayInWeek.Sunday }
        };
        #endregion

        private void Start()
        {
            InitializeTime();
            SubscribeToEvents();
        }

        private void InitializeTime()
        {
            currentHour = DateTime.Now.Hour;
            currentTimeOfDay = GetCurrentTimeOfDay();
        }

        private void SubscribeToEvents()
        {
            if (AuthenticationManager.Instance != null)
            {
                AuthenticationManager.Instance.OnNewUserCreate += OnNewUser;
            }
            else
            {
                Debug.LogWarning("AuthenticationManager.Instance is null. Cannot subscribe to OnNewUserCreate event.");
            }
        }

        private void OnDestroy()
        {
            if (AuthenticationManager.Instance != null)
            {
                AuthenticationManager.Instance.OnNewUserCreate -= OnNewUser;
            }
        }

        private async void OnNewUser(bool isNew)
        {
            if (Utilities.IsConnectedToInternet())
            {
                await UpdateServerTime();
                CalculateOfflineTimeEnergy();
            }
        }
        
        public void CalculateOfflineTimeEnergy()
        {
            int totalRegenEnergy = 0;
            if (UserManager.Instance != null)
            {
                TimeSpan timeDifference =
                    ServerTime - UserManager.Instance.UserData.LastOnlineTimestamp.StringToDateTime();
                int minutesPassed = (int)timeDifference.TotalMinutes;
                if (minutesPassed > 0)
                {
                    UserManager.Instance.RestoreEnergy(minutesPassed);
                }
            }
        }

        public async Task<bool> UpdateServerTime()
        {
            try
            {
                var serverTimestamp = await FirebaseManager.Instance.FetchServerTime();
                DateTime fetchedTime = serverTimestamp.ToDateTime();
                
                serverTimeOffset = 0;
                serverTime = new SerializableDateTime(fetchedTime);
                IsTimeValid = true;
                Debug.Log($"Server time updated: {fetchedTime}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to update server time: {ex.Message}");
                return false;
            }
        }

        public void CheckNewDay(Timestamp serverTimestamp)
        {
            if (LastOnlineTime == default || UserManager.Instance == null || UserManager.Instance.UserData == null)
            {
                Debug.Log("Cannot check for new day: LastOnlineTime or UserData not initialized");
                return;
            }

            DateTime serverDateTime = serverTimestamp.ToDateTime();
            bool isNewDay = LastOnlineTime.Date != serverDateTime.Date;

            if (isNewDay)
            {
               
            }
        }

        private void Update()
        {
            // Debug code - should be removed in production
            if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.P))
            {
                UserManager.Instance?.ConsumeEnergy(10);
            }

            if (!Utilities.IsConnectedToInternet()) return;
            
            UpdateMinuteCounter();
            UpdateServerTimeOffset();
        }

        private void UpdateMinuteCounter()
        {
            minuteCounter += Time.deltaTime;
            if (minuteCounter >= dispatchInterval)
            {
                minuteCounter -= dispatchInterval;
                OnMinuteElapsed?.Invoke();

                UpdateHourIfNeeded();
            }
        }

        private void UpdateHourIfNeeded()
        {
            int newHour = DateTime.Now.Hour;
            if (newHour != currentHour)
            {
                currentHour = newHour;
                currentTimeOfDay = GetCurrentTimeOfDay();
                OnTimeChanged?.Invoke();
            }
        }

        private void UpdateServerTimeOffset()
        {
            if (IsTimeValid)
            {
                serverTimeOffset += Time.deltaTime;    
            }
        }

        public TimeOfDay GetCurrentTimeOfDay()
        {
            return currentHour switch
            {
                >= 6 and < 9 => TimeOfDay.Morning,
                >= 9 and < 12 => TimeOfDay.Midday,
                >= 12 and < 18 => TimeOfDay.Afternoon,
                >= 18 and < 21 => TimeOfDay.Evening,
                _ => TimeOfDay.Night
            };
        }

        public DayInWeek GetCurrentDayInWeek()
        {
            DateTime currentDate = DateTime.Now;
            DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
            
            if (DayMapping.TryGetValue(currentDayOfWeek, out DayInWeek dayInWeek))
            {
                return dayInWeek;
            }
            
            throw new ArgumentOutOfRangeException(nameof(currentDayOfWeek), "Invalid day of the week.");
        }
    }

    public enum TimeOfDay
    {
        Morning,    // from 6:00 to 9:00
        Midday,     // from 9:00 to 12:00
        Afternoon,  // from 12:00 to 18:00
        Evening,    // from 18:00 to 21:00
        Night       // from 21:00 to 6:00
    }
#endif