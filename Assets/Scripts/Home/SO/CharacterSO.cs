using System;
using System.Collections.Generic;
using UnityEngine;
using Match3.Enums;
using RoboRyanTron.SearchableEnum;

[CreateAssetMenu(fileName = "Game/Character", menuName = "Game/Character")]
public class CharacterSO : ScriptableObject
{
    [SearchableEnum]
    public CharacterID id;
    public Sprite sprite;
    public string displayName;
    public int age;
}
