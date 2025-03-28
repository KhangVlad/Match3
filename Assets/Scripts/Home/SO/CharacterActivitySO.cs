using UnityEngine;
using System;
using System.Collections.Generic;
using Match3.Enums;

[CreateAssetMenu(fileName = "Game/ActivityTime", menuName = "Game/ActivityTime")]
public class CharacterActivitySO : ScriptableObject
{
    public CharacterID id;
    public Sprite sprite;
    public ActivityInfo[] activityInfos;
    public Vector2Int homePosition;
    public DayInWeek dayOff;
    public int[] sympathyRequired; //index is the level, value is the sympathy required, use to calculate the heart level
}

[Serializable]
public class ActivityInfo
{
    public int startTime;
    public int endTime;
    public Vector2Int appearPosition;
    public DayInWeek dayOfWeek;
}



public enum DayInWeek  
{
    Monday = 2,
    Tuesday = 3,
    Wednesday = 4,
    Thursday = 5,
    Friday = 6,
    Saturday = 7,
    Sunday = 8,
    None=0
}