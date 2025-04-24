using System;

using UnityEngine.Video;



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