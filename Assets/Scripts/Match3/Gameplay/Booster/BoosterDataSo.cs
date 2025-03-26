using UnityEngine;
using RoboRyanTron.SearchableEnum;
namespace Match3
{
    [CreateAssetMenu(fileName = "Booster", menuName = "ChillHavenStory/Booster")]
    public class BoosterDataSo : ScriptableObject
    {
        [SearchableEnum]
        public BoosterID ID;
        public Sprite Icon;

        [TextArea(3, 5)]
        public string Description;
    }
}
