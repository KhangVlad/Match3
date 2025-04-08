using UnityEngine;
using Match3;
using UnityEngine.UI;

    public class SpinItem : MonoBehaviour
    {
        public BoosterID boosterID;
        public Image i;
    
        public void InitializeItem(BoosterDataSo so)
        {
            boosterID = so.ID;
            i.sprite = so.Icon;
        }
    }
