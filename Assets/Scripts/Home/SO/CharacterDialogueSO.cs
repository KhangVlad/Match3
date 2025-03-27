using UnityEngine;


[CreateAssetMenu(fileName = "Game/Dialogue", menuName = "Game/Dialogue")]
public class CharacterDialogueSO : ScriptableObject
{
    public CharacterID id;
    public string[] levelDialogues;
}