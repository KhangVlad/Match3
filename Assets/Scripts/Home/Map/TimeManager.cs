// using System;
// using UnityEngine;
// using System.Collections.Generic;
// using Match3.Enums;
// public class TimeManager : MonoBehaviour
// {
//     public static TimeManager Instance { get; private set; }
//     [SerializeField] private int currentHour = 25;
//
//     private TimeOfDay currentTimeOfDay;
//     public event Action OnTimeChanged;
//
//     [field: SerializeField] public DateTime LastOnlineTime { get; set; }
//     [field: SerializeField] public DateTime LoginTime { get; set; }
//
//     private static readonly Dictionary<DayOfWeek, DayInWeek> DayMapping = new()
//     {
//         { DayOfWeek.Monday, DayInWeek.Monday },
//         { DayOfWeek.Tuesday, DayInWeek.Tuesday },
//         { DayOfWeek.Wednesday, DayInWeek.Wednesday },
//         { DayOfWeek.Thursday, DayInWeek.Thursday },
//         { DayOfWeek.Friday, DayInWeek.Friday },
//         { DayOfWeek.Saturday, DayInWeek.Saturday },
//         { DayOfWeek.Sunday, DayInWeek.Sunday }
//     };
//
//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
//
//     private void Start()
//     {
//         currentHour = DateTime.Now.Hour;
//         currentTimeOfDay = GetCurrentTimeOfDay();
//     }
//
//     public TimeOfDay GetCurrentTimeOfDay()
//     {
//         return currentHour switch
//         {
//             >= 6 and < 9 => TimeOfDay.Morning,
//             >= 9 and < 12 => TimeOfDay.Midday,
//             >= 12 and < 18 => TimeOfDay.Afternoon,
//             >= 18 and < 21 => TimeOfDay.Evening,
//             _ => TimeOfDay.Night
//         };
//     }
//
//     public DayInWeek GetCurrentDayInWeek()
//     {
//         DateTime currentDate = DateTime.Now;
//         DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;
//         if (DayMapping.TryGetValue(currentDayOfWeek, out DayInWeek dayInWeek))
//         {
//             return dayInWeek;
//         }
//         else
//         {
//             throw new ArgumentOutOfRangeException(nameof(currentDayOfWeek), "Invalid day of the week.");
//         }
//     }
// }
//
// public enum TimeOfDay
// {
//     Morning, //from 6:00 to 9:00
//     Midday, //from 9:00 to 12:00
//     Afternoon, // from 12:00 to 18:00
//     Evening, //from 18:00 to 21:00
//     Night //from 21:00 to 6:00
// }

using System;
using UnityEngine;
using System.Collections.Generic;
using Firebase.Firestore;
using Match3.Enums;

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
    public static TimeManager Instance { get; private set; }
    [SerializeField] private int currentHour = 25;
    [SerializeField] private int maxEnergy = 25; // Maximum energy cap
    
    private TimeOfDay currentTimeOfDay;
    public event Action OnTimeChanged;
    
    // Minute counter and event
    [SerializeField] private float minuteCounter = 0f;
    [SerializeField] private float DispatchInterval = 60f; // Time in seconds to represent a minute
    public event Action OnMinuteElapsed;

    [SerializeField] private SerializableDateTime lastOnlineTime = new SerializableDateTime();
    [SerializeField] private SerializableDateTime loginTime = new SerializableDateTime();

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

    [field: SerializeField] public DateTime LastOnlineTime { get; set; }
    [field: SerializeField] public DateTime LoginTime { get; set; }

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
        currentHour = DateTime.Now.Hour;
        currentTimeOfDay = GetCurrentTimeOfDay();
    }

 

    public void CalculateOfflineTimeEnergy()
    {
        Debug.Log("AAAA");
        if (LastOnlineTime != default && UserManager.Instance != null && UserManager.Instance.UserData != null)
        {
            // Calculate minutes passed since last online
            TimeSpan timeDifference = LoginTime - LastOnlineTime;
            int minutesPassed = (int)timeDifference.TotalMinutes;
            if (minutesPassed > 0)
            {
                Debug.Log($"Restoring {minutesPassed} energy from offline time");
                UserManager.Instance.RestoreEnergy(minutesPassed);
            }
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
            UserManager.Instance.ResetDailyGift();
        }
        else
        {
            Debug.Log("old day");
        }
    }
    
    
    private void Update()
    {
        minuteCounter += Time.deltaTime;
        
        if (minuteCounter >= DispatchInterval)
        {
            minuteCounter -= DispatchInterval;
            OnMinuteElapsed?.Invoke();
            
            // Update hour if needed
            int newHour = DateTime.Now.Hour;
            if (newHour != currentHour)
            {
                currentHour = newHour;
                currentTimeOfDay = GetCurrentTimeOfDay();
                OnTimeChanged?.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            UserManager.Instance.ConsumeEnergy(10);
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
        else
        {
            throw new ArgumentOutOfRangeException(nameof(currentDayOfWeek), "Invalid day of the week.");
        }
    }

}

public enum TimeOfDay
{
    Morning, //from 6:00 to 9:00
    Midday, //from 9:00 to 12:00
    Afternoon, // from 12:00 to 18:00
    Evening, //from 18:00 to 21:00
    Night //from 21:00 to 6:00
}