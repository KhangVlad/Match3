using System;
using UnityEngine;
using System.Collections.Generic;
using Match3.Enums;
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    [SerializeField] private int currentHour = 25;

    private TimeOfDay currentTimeOfDay;
    public event Action OnTimeChanged;

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