using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Game/Color", menuName = "Game/Color")]
public class CharacterAppearanceSO : ScriptableObject
{
    public List<CharacterAppearance> characterAppearances;
    public List<HeartColor> heartColors;
}

[Serializable]
public class CharacterAppearance
{
    public CharacterID id;
    public Color color;
    public Vector2 heartPosition;
}

[Serializable]
public class HeartColor
{
    public Color color;
    public int level;
}