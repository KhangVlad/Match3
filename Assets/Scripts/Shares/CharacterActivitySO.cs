using UnityEngine;
using System;
using System.Collections.Generic;
using Match3.Enums;

namespace Match3.Shares
{
    [CreateAssetMenu(fileName = "Game/ActivityTime", menuName = "Game/ActivityTime")]
    public class CharacterActivitySO : ScriptableObject
    {
        public CharacterID id;
        public Sprite sprite;
        public ActivityInfo[] activityInfos;
        public Vector2Int homePosition;
        public DayInWeek dayOff;

        public int[]
            sympathyRequired; //index is the level, value is the sympathy required, use to calculate the heart level
    }

    [Serializable]
    public class ActivityInfo
    {
        public int startTime;
        public int endTime;
        public Vector2Int appearPosition;
        public DayInWeek dayOfWeek;
    }
}


