using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Game/Dialogue", menuName = "Game/Dialogue")]
public class CharacterDialogueSO : ScriptableObject
{
    public CharacterID id;
    public LevelDialogueData[] data;
}


[Serializable]
public class LevelDialogueData
{
    public string[] dialog;
}


