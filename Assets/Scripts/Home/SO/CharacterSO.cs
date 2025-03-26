using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Game/Character", menuName = "Game/Character")]
public class CharacterSO : ScriptableObject
{
    public CharacterID id;
    public Sprite sprite;
    public string displayName;
    public int age;
}





public enum CharacterID
{
    Lina = 1,
    John = 2,
    Mary = 3,
    Tom = 4,
    Sarah = 5,
}