using System;
using UnityEngine;
using Match3.Enums;





public class RunTimeDialogData
{
    public CharacterID id;
    public LevelDialogueData[] data;
    public string[] greetingDialogs;
    public string[] lowSympathyDialogs;
}

public class LevelDialogueData
{
    public string[] levelDialogs;
}