using UnityEngine;
using System.Collections.Generic;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }

    [SerializeField] private UserData _userData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeNewUserData();
    }


    private UserData InitializeNewUserData()
    {
        _userData = new UserData()
        {
            AvaiableBoosters = new()
            {
                new BoosterSlot(Match3.BoosterID.ColorBurst, 99),
                new BoosterSlot(Match3.BoosterID.BlastBomb, 99),
                new BoosterSlot(Match3.BoosterID.AxisBomb, 99),
            },
            EquipBooster = new()
            {
                new BoosterSlot(Match3.BoosterID.ExtraMove, 99),
                new BoosterSlot(Match3.BoosterID.FreeSwitch, 99),
                new BoosterSlot(Match3.BoosterID.Hammer, 99),
            }
        };

        return null;
    }
}

public class CharacterData
{
    public CharacterID CharacterID;
    public int Sympathy;
    public List<int> Stars;
}






