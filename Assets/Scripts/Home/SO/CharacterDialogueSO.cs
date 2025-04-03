using System;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Game/Dialogue", menuName = "Game/Dialogue")]
public class CharacterDialogueSO : ScriptableObject
{
    public CharacterID id;
    public LevelDialogueData[] data;
    public string[] greetingDialogs;
}


[Serializable]
public class LevelDialogueData
{
    public string[] levelDialogs;
}