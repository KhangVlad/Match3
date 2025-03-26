using UnityEngine;
using RoboRyanTron.SearchableEnum;

namespace Match3
{
    [CreateAssetMenu(fileName = "QuestData", menuName ="ChillHavenStory/QuestData")]
    public class QuestDataSO : ScriptableObject
    {
        [SearchableEnum]
        public QuestID QuestID;
        public Sprite Icon;
    }

}
