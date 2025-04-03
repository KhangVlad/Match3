using System;
using System.Collections.Generic;
using UnityEngine;
using Match3.Enums;
using RoboRyanTron.SearchableEnum;

namespace Match3.Shares
{
    [CreateAssetMenu(fileName = "Game/Character", menuName = "Game/Character")]
    public class CharacterDataSO : ScriptableObject
    {
        [SearchableEnum]
        public CharacterID id;
        public Sprite sprite;
        public string displayName;
        public int age;
    }

}

