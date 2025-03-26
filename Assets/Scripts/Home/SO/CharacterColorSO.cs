using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Game/Color", menuName = "Game/Color")]
public class CharacterAppearanceSO : ScriptableObject
{
    public List<CharacterAppearance> characterAppearances;
}

[Serializable]
public class CharacterAppearance
{
    public CharacterID id;
    public Color color;
    public Vector2 heartPosition;
}