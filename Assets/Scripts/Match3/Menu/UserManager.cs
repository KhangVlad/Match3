using UnityEngine;
using System.Collections.Generic;

namespace Match3
{
    public class UserManager : MonoBehaviour
    {
        public static UserManager Instance { get;private set; }
        public event System.Action<Booster> OnSelectGameplayBooster;
        public event System.Action OnUnselectGameplayBooster;


        [Header("~Runtime")]
        public List<Booster> AvaiableBoosters;
        public List<Booster> EquipBoosters;
        [Space(10)]
        public List<Booster> GameplayBoosters;
        public Booster SelectedGameplayBooster {get; private set;}


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            
            LoadAvaiableBooster();
            LoadGameplayBooster();
        }


        private void LoadAvaiableBooster()
        {
            EquipBoosters = new();
            AvaiableBoosters = new();

            ColorBurstBooster colorBurstBooster = new ColorBurstBooster(99);
            BlastBombBooster blastBombBooster = new BlastBombBooster(99);
            AxisBombBooster axisBombBooster = new AxisBombBooster(99);

            AvaiableBoosters.Add(colorBurstBooster);
            AvaiableBoosters.Add(blastBombBooster);
            AvaiableBoosters.Add(axisBombBooster);
        }


        private void LoadGameplayBooster()
        {
            GameplayBoosters = new();

            ExtraMoveBooster extraMoveBooster = new ExtraMoveBooster(99);
            FreeSwitchBooster freeSwitchBooster = new FreeSwitchBooster(99);
            HammerBooster hammerBooster = new HammerBooster(99);

            GameplayBoosters.Add(extraMoveBooster);
            GameplayBoosters.Add(freeSwitchBooster);
            GameplayBoosters.Add(hammerBooster);
        }

        public void EquipBoosterByID(BoosterID boosterID)
        {
            bool canEquip = true;

            for(int i = 0; i < EquipBoosters.Count; i++)
            {
                if (EquipBoosters[i].BoosterID == boosterID) 
                {
                    canEquip = false;
                    break;
                }
            }

            if(canEquip)
            {
                for (int i = 0; i < AvaiableBoosters.Count; i++)
                {
                    if (AvaiableBoosters[i].BoosterID == boosterID)
                    {
                        EquipBoosters.Add(AvaiableBoosters[i]);
                        return;
                    }
                }
            }
         

            Debug.LogError($"Not found booster ID: {boosterID}");
        }

        public void UnequipBoosterByID(BoosterID boosterID)
        {
            bool canUnequip = false;
            int unequipIndex = 0;

            for (int i = 0; i < EquipBoosters.Count; i++)
            {
                if (EquipBoosters[i].BoosterID == boosterID)
                {
                    canUnequip = true;
                    unequipIndex = i;
                    break;
                }
            }

            if(canUnequip)
            {
                EquipBoosters.RemoveAt(unequipIndex);
            }
        }

        public bool IsBoosterEquipped(BoosterID id)
        {
            for(int i = 0; i < EquipBoosters.Count; i++)
            {
                if (EquipBoosters[i].BoosterID == id)
                {
                    return true;
                }
            }
            return false;
        }


        public void SelectGameplayBooster(BoosterID boosterID)
        {
            for(int i = 0; i < GameplayBoosters.Count; i++)
            {
                if (GameplayBoosters[i].BoosterID==boosterID)
                {
                    SelectedGameplayBooster = GameplayBoosters[i];
                    OnSelectGameplayBooster?.Invoke(SelectedGameplayBooster);
                }
            }
        }

        public void UnselectGameplayBooster()
        {
            SelectedGameplayBooster = null;
            OnUnselectGameplayBooster?.Invoke();
        }
    }
    
    
}
