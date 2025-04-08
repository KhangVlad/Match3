using System;
using UnityEngine;
using Match3.Enums;
using UnityEngine.Serialization;
using Match3.Enums;

[CreateAssetMenu(fileName = "Game/Dialogue", menuName = "Game/Dialogue")]
public class CharacterDialogueSO : ScriptableObject
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