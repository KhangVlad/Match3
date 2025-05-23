using System;
using UnityEngine;

namespace DynamicScrollRect
{
    [Serializable]
    public class ScrollItemData
    {
        // public int Index { get; }

        // int ind, bool l, int[,] quest, int total

        public int Index { get; }
        public bool IsLocked;
        public int[,] Quest;
        public int Total;
        public ScrollItemData( int index, bool isLocked, int[,] quest, int total)
        {
            Index = index;
            IsLocked = isLocked;
            Quest = quest;
            Total = total;
        }
        // {
        //     Index = index;
        // }
    }
}
