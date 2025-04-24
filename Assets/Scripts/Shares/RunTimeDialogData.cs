using System;
using UnityEngine;
using Match3.Enums;




[Serializable]
public class RunTimeDialogData
{
    public CharacterID id;
    public LevelDialogueData[] data;
    public string[] greetingDialogs;
    public string[] lowSympathyDialogs;
}

[Serializable]
public class LevelDialogueData
{
    public string[] levelDialogs;
}