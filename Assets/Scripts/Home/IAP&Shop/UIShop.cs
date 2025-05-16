using UnityEngine;
using Match3;
using UnityEngine.UI;
using System.Linq;

public class UIShop : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Button _closeBtn;

    [Header("Shop Pack")] [SerializeField] private Transform _contentParent;

    // [SerializeField] private UIShopPack[] _uiShopPackPrefab;
    [SerializeField] private UIShopPack[] _uiPackSlots;
    [SerializeField] private GameObject _uiSingleUIShopPackPrefab; //contain 1 uishop pack
    [SerializeField] private GameObject _uiPairUIShopPackPrefab; //contain 2 uishoppackw
    [SerializeField] private GameObject _uiThreeUIShopPackPrefab;

    [SerializeField] private Sprite goldPack;
    [SerializeField] private Sprite energyPack;
    [SerializeField] private Sprite goldAndBlastBomb;
    [SerializeField] private Sprite goldAndAxisAndEnergy;
    [SerializeField] private Sprite goldAndFullBooster;
    [SerializeField] private Sprite comboBoosters;

    private Sprite MainSpritePicker(ShopItemPack pack)
    {
        if (pack == null || pack.Items == null || pack.Items.Count == 0)
            return null;

        System.Collections.Generic.List<ShopItemID> boosterIds = new System.Collections.Generic.List<ShopItemID>();
        bool hasGold = false;
        bool hasEnergy = false;
        bool hasRemoveAds = false;

        foreach (var item in pack.Items)
        {
            switch (item.ShopItemID)
            {
                case ShopItemID.Gold:
                    hasGold = true;
                    boosterIds.Add(ShopItemID.Gold);
                    break;
                case ShopItemID.RemoveAds:
                    hasRemoveAds = true;
                    break;
                case ShopItemID.Energy:
                    hasEnergy = true;
                    boosterIds.Add(ShopItemID.Energy);
                    break;
                case ShopItemID.ColorBurst:
                case ShopItemID.BlastBomb:
                case ShopItemID.AxisBomb:
                case ShopItemID.ExtraMove:
                case ShopItemID.FreeSwitch:
                case ShopItemID.Hammer:
                    boosterIds.Add(item.ShopItemID);
                    break;
            }
        }

        int boosterTypeCount = boosterIds.Count;
        bool isFullBoosterPack = boosterTypeCount >= 4;

        bool hasBlastBomb = boosterIds.Contains(ShopItemID.BlastBomb);
        bool hasAxisBomb = boosterIds.Contains(ShopItemID.AxisBomb);

        // Return appropriate sprite based on combinations
        if (isFullBoosterPack && hasGold)
            return goldAndFullBooster;

        if (hasGold && hasAxisBomb && hasEnergy)
            return goldAndAxisAndEnergy;

        if (hasGold && hasBlastBomb)
            return goldAndBlastBomb;

        if (!hasGold && !hasEnergy && !hasRemoveAds && boosterTypeCount > 0)
            return comboBoosters;

        if (hasEnergy)
            return energyPack;

        if (hasGold)
            return goldPack;

        // Default case if no specific combination matches
        return comboBoosters;
    }

    private ShopType currentShopType = ShopType.Purchase;

    private enum ShopType
    {
        Purchase,
        Gold
    }

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        _closeBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayButtonSfx();
            ActiveCanvas(false);
        });
        LoadShop();
    }

    private void OnDestroy()
    {
        _closeBtn.onClick.RemoveAllListeners();
    }

    public void ActiveCanvas(bool enable)
    {
        this._canvas.enabled = enable;
    }


    private void LoadShop()
    {
        ClearShopItems();
        System.Collections.Generic.List<UIShopPack> allShopPacks = new System.Collections.Generic.List<UIShopPack>();
        ProcessShopItems(ShopManager.Instance.ShopData.Purchase_Shop, allShopPacks);
        ProcessShopItems(ShopManager.Instance.ShopData.Gold_Shop, allShopPacks);
        _uiPackSlots = allShopPacks.ToArray();
    }


    private void ClearShopItems()
    {
        // Destroy all existing shop items
        if (_uiPackSlots != null)
        {
            foreach (var slot in _uiPackSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
        }
    }

    // private void ProcessShopItems(System.Collections.Generic.List<ShopItemPack> shopItems,
    //     System.Collections.Generic.List<UIShopPack> allShopPacks)
    // {
    //     // Count items by type
    //     int totalSingle = shopItems.Count(x => x.ShopPackType == ShopPackType.Single);
    //     int totalPair = shopItems.Count(x => x.ShopPackType == ShopPackType.Pair);
    //     int totalTriple = shopItems.Count(x => x.ShopPackType == ShopPackType.Triple);
    //
    //     // Calculate how many prefabs of each type we need
    //     int singlePrefabsNeeded = totalSingle / 1 + (totalSingle % 1 > 0 ? 1 : 0);
    //     int pairPrefabsNeeded = totalPair / 2 + (totalPair % 2 > 0 ? 1 : 0);
    //     int triplePrefabsNeeded = totalTriple / 3 + (totalTriple % 3 > 0 ? 1 : 0);
    //
    //     // Process single packs
    //     int singlePacksProcessed = 0;
    //     for (int i = 0; i < singlePrefabsNeeded; i++)
    //     {
    //         GameObject singlePrefabInstance = Instantiate(_uiSingleUIShopPackPrefab, _contentParent);
    //         UIShopPack packSlot = singlePrefabInstance.GetComponentInChildren<UIShopPack>();
    //
    //         if (packSlot != null && singlePacksProcessed < totalSingle)
    //         {
    //             var pack = shopItems.Where(x => x.ShopPackType == ShopPackType.Single)
    //                 .Skip(singlePacksProcessed).FirstOrDefault();
    //             if (pack != null)
    //             {
    //                 packSlot.SetPackData(pack, MainSpritePicker(pack));
    //                 allShopPacks.Add(packSlot);
    //                 singlePacksProcessed++;
    //             }
    //         }
    //     }
    //
    //     // Process pair packs
    //     int pairPacksProcessed = 0;
    //     for (int i = 0; i < pairPrefabsNeeded; i++)
    //     {
    //         GameObject pairPrefabInstance = Instantiate(_uiPairUIShopPackPrefab, _contentParent);
    //         UIShopPack[] packSlots = pairPrefabInstance.GetComponentsInChildren<UIShopPack>();
    //
    //         for (int j = 0; j < packSlots.Length && pairPacksProcessed < totalPair; j++)
    //         {
    //             var pack = shopItems.Where(x => x.ShopPackType == ShopPackType.Pair)
    //                 .Skip(pairPacksProcessed).FirstOrDefault();
    //             if (pack != null)
    //             {
    //                 packSlots[j].SetPackData(pack,MainSpritePicker(pack));
    //                 allShopPacks.Add(packSlots[j]);
    //                 pairPacksProcessed++;
    //             }
    //         }
    //     }
    //
    //     // Process triple packs
    //     int triplePacksProcessed = 0;
    //     for (int i = 0; i < triplePrefabsNeeded; i++)
    //     {
    //         GameObject triplePrefabInstance = Instantiate(_uiThreeUIShopPackPrefab, _contentParent);
    //         UIShopPack[] packSlots = triplePrefabInstance.GetComponentsInChildren<UIShopPack>();
    //
    //         for (int j = 0; j < packSlots.Length && triplePacksProcessed < totalTriple; j++)
    //         {
    //             var pack = shopItems.Where(x => x.ShopPackType == ShopPackType.Triple)
    //                 .Skip(triplePacksProcessed).FirstOrDefault();
    //             if (pack != null)
    //             {
    //                 packSlots[j].SetPackData(pack,MainSpritePicker(pack));
    //                 allShopPacks.Add(packSlots[j]);
    //                 triplePacksProcessed++;
    //             }
    //         }
    //     }
    // }
    private void ProcessShopItems(System.Collections.Generic.List<ShopItemPack> shopItems,
    System.Collections.Generic.List<UIShopPack> allShopPacks)
{
    // Count items by type
    int totalSingle = shopItems.Count(x => x.ShopPackType == ShopPackType.Single);
    int totalPair = shopItems.Count(x => x.ShopPackType == ShopPackType.Pair);
    int totalTriple = shopItems.Count(x => x.ShopPackType == ShopPackType.Triple);

    // Calculate how many prefabs of each type we need
    int singlePrefabsNeeded = totalSingle / 1 + (totalSingle % 1 > 0 ? 1 : 0);
    int pairPrefabsNeeded = totalPair / 2 + (totalPair % 2 > 0 ? 1 : 0);
    int triplePrefabsNeeded = totalTriple / 3 + (totalTriple % 3 > 0 ? 1 : 0);

    // Process single packs
    int singlePacksProcessed = 0;
    for (int i = 0; i < singlePrefabsNeeded; i++)
    {
        GameObject singlePrefabInstance = Instantiate(_uiSingleUIShopPackPrefab, _contentParent);
        UIShopPack packSlot = singlePrefabInstance.GetComponentInChildren<UIShopPack>();

        if (packSlot != null && singlePacksProcessed < totalSingle)
        {
            var pack = shopItems.Where(x => x.ShopPackType == ShopPackType.Single)
                .Skip(singlePacksProcessed).FirstOrDefault();
            if (pack != null)
            {
                packSlot.SetPackData(pack, MainSpritePicker(pack));
                allShopPacks.Add(packSlot);
                singlePacksProcessed++;
            }
        }
    }

    // Process pair packs
    int pairPacksProcessed = 0;
    for (int i = 0; i < pairPrefabsNeeded; i++)
    {
        GameObject pairPrefabInstance = Instantiate(_uiPairUIShopPackPrefab, _contentParent);
        UIShopPack[] packSlots = pairPrefabInstance.GetComponentsInChildren<UIShopPack>();

        // Process each slot in the pair prefab
        for (int j = 0; j < packSlots.Length; j++)
        {
            if (pairPacksProcessed < totalPair)
            {
                var pack = shopItems.Where(x => x.ShopPackType == ShopPackType.Pair)
                    .Skip(pairPacksProcessed).FirstOrDefault();
                if (pack != null)
                {
                    packSlots[j].SetPackData(pack, MainSpritePicker(pack));
                    allShopPacks.Add(packSlots[j]);
                    pairPacksProcessed++;
                }
            }
            else
            {
                // Disable or destroy the empty slot
                Destroy(packSlots[j].gameObject);
            }
        }
    }

    // Process triple packs
    int triplePacksProcessed = 0;
    for (int i = 0; i < triplePrefabsNeeded; i++)
    {
        GameObject triplePrefabInstance = Instantiate(_uiThreeUIShopPackPrefab, _contentParent);
        UIShopPack[] packSlots = triplePrefabInstance.GetComponentsInChildren<UIShopPack>();

        // Process each slot in the triple prefab
        for (int j = 0; j < packSlots.Length; j++)
        {
            if (triplePacksProcessed < totalTriple)
            {
                var pack = shopItems.Where(x => x.ShopPackType == ShopPackType.Triple)
                    .Skip(triplePacksProcessed).FirstOrDefault();
                if (pack != null)
                {
                    packSlots[j].SetPackData(pack, MainSpritePicker(pack));
                    allShopPacks.Add(packSlots[j]);
                    triplePacksProcessed++;
                }
            }
            else
            {
                // Disable or destroy the empty slot
                Destroy(packSlots[j].gameObject);
            }
        }
    }
}
}