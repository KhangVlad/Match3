using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Game/Color", menuName = "Game/Color")]
public class CharacterAppearanceSO : ScriptableObject
{
    public List<CharacterAppearance> AppearancesInfo;
    public Color[] heartColors;
}

[Serializable]
public class CharacterAppearance
{
    public CharacterID id;
    public Color panelColor;
    public Vector2 heartPosition;
}