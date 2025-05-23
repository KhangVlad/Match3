
using UnityEngine;
using Match3.Enums;
using RoboRyanTron.SearchableEnum;

namespace Match3.Shares
{
    [CreateAssetMenu(fileName = "Game/Character", menuName = "Game/Character")]
    public class CharacterDataSO : ScriptableObject
    {
        [SearchableEnum] public CharacterID id;
        public Sprite sprite;
        public string displayName;
        public int age;
        public int[] heartLevelRequired; // index is the level, value is the heart level required, use to calculate the sympathy required
        public int TotalHeartToUnlock; // total heart to unlock the character



        public int CurrentSympathyLevel(int totalHeart)
        {
            if (heartLevelRequired == null || heartLevelRequired.Length == 0)
                return 0;

            for (int i = heartLevelRequired.Length - 1; i >= 0; i--)
            {
                if (totalHeart >= heartLevelRequired[i])
                {
                    return i;
                }
            }

            return 0;
        }

    }
}