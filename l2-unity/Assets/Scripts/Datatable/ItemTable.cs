using System.Collections.Generic;
using UnityEngine;

public class ItemTable
{
    private static ItemTable _instance;
    public static ItemTable Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ItemTable();
            }

            return _instance;
        }
    }

    [SerializeField] private List<int> weaponsToLoad = new List<int>();
    [SerializeField] private List<int> armorsToLoad = new List<int>();
    [SerializeField] private List<int> itemsToLoad = new List<int>();

    private Dictionary<int, Weapon> _weapons = new Dictionary<int, Weapon>();
    private Dictionary<int, Armor> _armors = new Dictionary<int, Armor>();
    private Dictionary<int, EtcItem> _etcItems = new Dictionary<int, EtcItem>();

    public Dictionary<int, Weapon> Weapons { get { return _weapons; } }
    public Dictionary<int, Armor> Armors { get { return _armors; } }
    public Dictionary<int, EtcItem> EtcItems { get { return _etcItems; } }

    public static int NAKED_CHEST = 21;
    public static int NAKED_LEGS = 28;
    public static int NAKED_GLOVES = 48;
    public static int NAKED_BOOTS = 35;
    private bool _loadAll = true;

    public void Initialize()
    {
        if (!_loadAll)
        {
            FillDataToLoad();
        }
    }

    // private void OnDestroy()
    // {
    //     _weapons.Clear();
    //     _armors.Clear();
    //     _instance = null;
    // }

    public void CacheItems()
    {
        CacheWeapons();
        CacheArmors();
        CacheEtcItems();
    }

    private void FillDataToLoad()
    {
        weaponsToLoad = new List<int> { 1, 2, 3, 4, 5, 6, 7, 10, 14, 89, 102, 129, 177, 188, 156, 275, 2369, 2370, 5284, 20 };
        armorsToLoad = new List<int> { NAKED_CHEST, NAKED_LEGS, NAKED_GLOVES, NAKED_BOOTS, 425, 461, 1146, 1147, 45, 118, 112, 116 };
        itemsToLoad = new List<int>() { 1835, 3947, 2509, 57, 736, 5588, 17 };
    }

    public bool ShouldLoadItem(int id)
    {
        if (_loadAll) return true;

        if (weaponsToLoad.Count == 0 && armorsToLoad.Count == 0 && itemsToLoad.Count == 0)
        {
            return true;
        }

        if (weaponsToLoad.Contains(id))
        {
            return true;
        }
        if (armorsToLoad.Contains(id))
        {
            return true;
        }
        if (itemsToLoad.Contains(id))
        {
            return true;
        }

        return false;
    }

    private void CacheWeapons()
    {
        foreach (KeyValuePair<int, Weapongrp> kvp in WeapongrpTable.Instance.WeaponGrps)
        {
            if (_weapons.ContainsKey(kvp.Key))
            {
                continue;
            }

            Weapongrp weapongrp = kvp.Value;
            ItemName itemName = ItemNameTable.Instance.GetItemName(kvp.Key);
            ItemStatData itemStatData = ItemStatDataTable.Instance.GetItemStatData(kvp.Key);

            Weapon weapon = new Weapon(kvp.Key, itemName, itemStatData, weapongrp);

            _weapons.TryAdd(kvp.Key, weapon);

        }
    }

    private void CacheArmors()
    {
        foreach (KeyValuePair<int, Armorgrp> kvp in ArmorgrpTable.Instance.ArmorGrps)
        {
            if (_armors.ContainsKey(kvp.Key))
            {
                continue;
            }

            Armorgrp armorgrp = kvp.Value;
            ItemName itemName = ItemNameTable.Instance.GetItemName(kvp.Key);
            ItemStatData itemStatData = ItemStatDataTable.Instance.GetItemStatData(kvp.Key);

            Armor armor = new Armor(kvp.Key, itemName, itemStatData, armorgrp);
            _armors.TryAdd(kvp.Key, armor);
        }
    }

    private void CacheEtcItems()
    {
        foreach (KeyValuePair<int, EtcItemgrp> kvp in EtcItemgrpTable.Instance.EtcItemGrps)
        {
            if (_etcItems.ContainsKey(kvp.Key))
            {
                continue;
            }

            EtcItemgrp itemgrp = kvp.Value;
            ItemName itemName = ItemNameTable.Instance.GetItemName(kvp.Key);
            ItemStatData itemStatData = ItemStatDataTable.Instance.GetItemStatData(kvp.Key);

            EtcItem item = new EtcItem(kvp.Key, itemName, itemStatData, itemgrp);
            _etcItems.TryAdd(kvp.Key, item);
        }
    }

    private static readonly Dictionary<int, (string name, ItemSlot slot, bool isWeapon)> _hellsingCustomItems = new Dictionary<int, (string, ItemSlot, bool)>
    {
        { 79905, ("Hellsing Custom Weapon", ItemSlot.SLOT_R_HAND, true) },
        { 9300, ("Draconic Golden Armor", ItemSlot.SLOT_CHEST, false) },
        { 9301, ("Draconic Golden Gloves", ItemSlot.SLOT_GLOVES, false) },
        { 9302, ("Draconic Golden Boots", ItemSlot.SLOT_FEET, false) },
        { 9303, ("Draconic Golden Helmet", ItemSlot.SLOT_HEAD, false) },
        { 9304, ("Valakas Wings Golden", ItemSlot.SLOT_HEAD, false) },
        { 9305, ("Valakas Wings Golden", ItemSlot.SLOT_HEAD, false) },
        { 9306, ("Baium Dread", ItemSlot.SLOT_HEAD, false) },
        { 9307, ("Akamanah Helmet", ItemSlot.SLOT_HEAD, false) },
        { 9308, ("Draconic Golden Boots", ItemSlot.SLOT_FEET, false) },
        { 9309, ("Draconic Golden Gloves", ItemSlot.SLOT_GLOVES, false) },
        { 9310, ("Draconic Golden Armor", ItemSlot.SLOT_CHEST, false) },
        { 9314, ("Dynasty Tunic", ItemSlot.SLOT_CHEST, false) },
        { 9315, ("Dynasty Tunic", ItemSlot.SLOT_CHEST, false) },
        { 9316, ("Dynasty Tunic", ItemSlot.SLOT_CHEST, false) },
        { 9317, ("Dynasty Gaiters", ItemSlot.SLOT_LEGS, false) },
        { 9318, ("Dynasty Helmet", ItemSlot.SLOT_HEAD, false) },
        { 9319, ("Dynasty Gauntlets", ItemSlot.SLOT_GLOVES, false) },
        { 9320, ("Dynasty Boots", ItemSlot.SLOT_FEET, false) },
        { 9321, ("Dynasty Leather Leggings", ItemSlot.SLOT_LEGS, false) },
        { 9322, ("Dynasty Leather Helmet", ItemSlot.SLOT_HEAD, false) },
        { 9323, ("Dynasty Leather Gloves", ItemSlot.SLOT_GLOVES, false) },
        { 9324, ("Dynasty Leather Boots", ItemSlot.SLOT_FEET, false) },
        { 9325, ("Dynasty Stockings", ItemSlot.SLOT_LEGS, false) },
        { 9326, ("Dynasty Circlet", ItemSlot.SLOT_HEAD, false) },
        { 9327, ("Dynasty Gloves", ItemSlot.SLOT_GLOVES, false) },
        { 9328, ("Dynasty Shoes", ItemSlot.SLOT_FEET, false) },
        { 9329, ("Dynasty Breastplate", ItemSlot.SLOT_CHEST, false) },
        { 9330, ("Dynasty Breastplate", ItemSlot.SLOT_CHEST, false) },
        { 9331, ("Dynasty Leather Gloves", ItemSlot.SLOT_GLOVES, false) },
        { 9332, ("Dynasty Leggings", ItemSlot.SLOT_LEGS, false) },
        { 9333, ("Dynasty Tunic", ItemSlot.SLOT_CHEST, false) },
        { 9334, ("Dynasty Tunic", ItemSlot.SLOT_CHEST, false) },
        { 9561, ("Vesper Cutter", ItemSlot.SLOT_R_HAND, true) },
        { 9562, ("Vesper Shaper", ItemSlot.SLOT_R_HAND, true) },
        { 9563, ("Vesper Buster", ItemSlot.SLOT_R_HAND, true) },
        { 9564, ("Vesper Slasher", ItemSlot.SLOT_LR_HAND, true) },
        { 9565, ("Vesper Stormer", ItemSlot.SLOT_LR_HAND, true) },
        { 9566, ("Vesper Avenger", ItemSlot.SLOT_R_HAND, true) },
        { 9567, ("Vesper Fighter", ItemSlot.SLOT_LR_HAND, true) },
        { 9568, ("Vesper Caster", ItemSlot.SLOT_R_HAND, true) },
        { 9569, ("Vesper Thrower", ItemSlot.SLOT_LR_HAND, true) },
        { 14638, ("Valakas Shield", ItemSlot.SLOT_L_HAND, true) },
        { 14639, ("Zaken Shield", ItemSlot.SLOT_L_HAND, true) },
        { 14650, ("Baium Shield", ItemSlot.SLOT_L_HAND, true) },
        { 14651, ("Beleth Shield", ItemSlot.SLOT_L_HAND, true) },
        { 14652, ("Lilith Shield", ItemSlot.SLOT_L_HAND, true) },
        { 14653, ("Orfen Shield", ItemSlot.SLOT_L_HAND, true) }
    };

    private Dictionary<(int, ItemSlot), Armor> _customSlotArmors = new Dictionary<(int, ItemSlot), Armor>();

    public Weapon GetWeapon(int id)
    {
        if (id <= 0) return null;

        if (_weapons.TryGetValue(id, out Weapon weapon) && weapon != null)
        {
            return weapon;
        }

        // Synthesize fallback weapon for custom Hellsing weapon IDs
        int templateId = 129; // Sword of Revolution (guaranteed in Resources)
        if (_hellsingCustomItems.TryGetValue(id, out var customMeta) && customMeta.slot == ItemSlot.SLOT_L_HAND)
        {
            templateId = 625; // Bone Shield / Tower Shield
        }
        if (!_weapons.TryGetValue(templateId, out Weapon templateWeapon) || templateWeapon == null)
        {
            _weapons.TryGetValue(2, out templateWeapon); // Long Sword fallback
        }
        if (templateWeapon == null)
        {
            foreach (var w in _weapons.Values)
            {
                if (w != null) { templateWeapon = w; break; }
            }
        }
        if (templateWeapon == null) return null;

        string customName = _hellsingCustomItems.TryGetValue(id, out var meta) ? meta.name : $"Hellsing Weapon #{id}";
        ItemName itemName = ItemNameTable.Instance.GetItemName(id) ?? new ItemName
        {
            Id = id,
            Name = customName,
            Description = "Custom Hellsing Server Equipment.",
            DefaultAction = "equip",
            Tradeable = true,
            Droppable = true,
            Destructible = true,
            Sellable = true
        };
        ItemStatData statData = ItemStatDataTable.Instance.GetItemStatData(id) ?? templateWeapon.ItemStatData ?? new ItemStatData { ObjectId = id, PAtk = 250, MAtk = 180, PAtkSpd = 379 };

        Weapongrp srcGrp = templateWeapon.Weapongrp;
        Weapongrp grp = new Weapongrp
        {
            ObjectId = id,
            BodyPart = srcGrp.BodyPart,
            WeaponType = srcGrp.WeaponType,
            Models = srcGrp.Models,
            Textures = srcGrp.Textures,
            Icon = srcGrp.Icon,
            EquipSound = srcGrp.EquipSound,
            DropSound = srcGrp.DropSound,
            ItemSounds = srcGrp.ItemSounds,
            Grade = ItemGrade.s,
            Weight = srcGrp.Weight,
            Soulshot = 1,
            Spiritshot = 1
        };

        weapon = new Weapon(id, itemName, statData, grp);
        _weapons[id] = weapon;
        return weapon;
    }

    public Armor GetArmor(int id)
    {
        return GetArmor(id, ItemSlot.SLOT_NONE);
    }

    public Armor GetArmor(int id, ItemSlot preferredSlot)
    {
        if (id <= 0) return null;

        if (_armors.TryGetValue(id, out Armor armor) && armor != null)
        {
            if (preferredSlot == ItemSlot.SLOT_NONE || armor.Armorgrp.BodyPart == preferredSlot ||
                (preferredSlot == ItemSlot.SLOT_CHEST && armor.Armorgrp.BodyPart == ItemSlot.SLOT_FULL_ARMOR))
            {
                return armor;
            }
            if (id < 9000)
            {
                return armor;
            }
        }

        ItemSlot targetSlot = preferredSlot;
        string customName = $"Hellsing Armor #{id}";
        if (_hellsingCustomItems.TryGetValue(id, out var meta))
        {
            customName = meta.name;
            if (targetSlot == ItemSlot.SLOT_NONE)
            {
                targetSlot = meta.slot;
            }
        }
        if (targetSlot == ItemSlot.SLOT_NONE)
        {
            targetSlot = ItemSlot.SLOT_CHEST;
        }

        if (_customSlotArmors.TryGetValue((id, targetSlot), out Armor cachedSlotArmor))
        {
            return cachedSlotArmor;
        }

        // Pick a template armor with loaded 3D meshes in Resources
        int templateId = NAKED_CHEST;
        switch (targetSlot)
        {
            case ItemSlot.SLOT_CHEST:
            case ItemSlot.SLOT_FULL_ARMOR:
                templateId = _armors.ContainsKey(1146) ? 1146 : (_armors.ContainsKey(425) ? 425 : NAKED_CHEST);
                break;
            case ItemSlot.SLOT_LEGS:
                templateId = _armors.ContainsKey(1147) ? 1147 : (_armors.ContainsKey(461) ? 461 : NAKED_LEGS);
                break;
            case ItemSlot.SLOT_GLOVES:
                templateId = _armors.ContainsKey(48) ? 48 : NAKED_GLOVES;
                break;
            case ItemSlot.SLOT_FEET:
                templateId = _armors.ContainsKey(35) ? 35 : NAKED_BOOTS;
                break;
            case ItemSlot.SLOT_HEAD:
                templateId = _armors.ContainsKey(45) ? 45 : NAKED_CHEST;
                break;
            case ItemSlot.SLOT_NECK:
                templateId = _armors.ContainsKey(118) ? 118 : NAKED_CHEST;
                break;
            case ItemSlot.SLOT_R_EAR:
            case ItemSlot.SLOT_L_EAR:
                templateId = _armors.ContainsKey(112) ? 112 : NAKED_CHEST;
                break;
            case ItemSlot.SLOT_R_FINGER:
            case ItemSlot.SLOT_L_FINGER:
                templateId = _armors.ContainsKey(116) ? 116 : NAKED_CHEST;
                break;
        }

        // Try to find a higher-tier armor in _armors with the same targetSlot whose model is loaded in ModelTable
        Armor templateArmor = null;
        _armors.TryGetValue(templateId, out templateArmor);
        foreach (var kvp in _armors)
        {
            if (kvp.Key != NAKED_CHEST && kvp.Key != NAKED_LEGS && kvp.Key != NAKED_GLOVES && kvp.Key != NAKED_BOOTS
                && kvp.Value != null && kvp.Value.Armorgrp != null && kvp.Value.Armorgrp.BodyPart == targetSlot)
            {
                if (ModelTable.Instance.HasLoadedArmorModel(kvp.Value))
                {
                    templateArmor = kvp.Value;
                    if (kvp.Key >= 60) break; // Prefer higher-tier visible armor set
                }
            }
        }

        if (templateArmor == null)
        {
            foreach (var a in _armors.Values)
            {
                if (a != null) { templateArmor = a; break; }
            }
        }
        if (templateArmor == null) return null;

        ItemName itemName = ItemNameTable.Instance.GetItemName(id) ?? new ItemName
        {
            Id = id,
            Name = customName,
            Description = "Custom Hellsing Server Armor.",
            DefaultAction = "equip",
            Tradeable = true,
            Droppable = true,
            Destructible = true,
            Sellable = true
        };
        ItemStatData statData = ItemStatDataTable.Instance.GetItemStatData(id) ?? templateArmor.ItemStatData ?? new ItemStatData { ObjectId = id, PDef = 180, MDef = 120 };

        Armorgrp srcGrp = templateArmor.Armorgrp;
        Armorgrp grp = new Armorgrp
        {
            ObjectId = id,
            BodyPart = targetSlot,
            Model = srcGrp.Model,
            Texture = srcGrp.Texture,
            Icon = srcGrp.Icon,
            EquipSound = srcGrp.EquipSound,
            Grade = ItemGrade.s,
            Weight = srcGrp.Weight,
            MpBonus = srcGrp.MpBonus
        };

        Armor synthesized = new Armor(id, itemName, statData, grp);
        _customSlotArmors[(id, targetSlot)] = synthesized;
        if (!_armors.ContainsKey(id))
        {
            _armors[id] = synthesized;
        }
        return synthesized;
    }

    public EtcItem GetEtcItem(int id)
    {
        if (id <= 0) return null;

        if (_etcItems.TryGetValue(id, out EtcItem item) && item != null)
        {
            return item;
        }

        EtcItem templateItem = null;
        if (!_etcItems.TryGetValue(57, out templateItem))
        {
            foreach (var e in _etcItems.Values)
            {
                if (e != null) { templateItem = e; break; }
            }
        }
        if (templateItem == null) return null;

        ItemName itemName = ItemNameTable.Instance.GetItemName(id) ?? new ItemName
        {
            Id = id,
            Name = $"Item #{id}",
            Description = "Custom Hellsing Server Item.",
            DefaultAction = "none",
            Tradeable = true,
            Droppable = true,
            Destructible = true,
            Sellable = true
        };
        ItemStatData statData = ItemStatDataTable.Instance.GetItemStatData(id) ?? templateItem.ItemStatData ?? new ItemStatData { ObjectId = id };
        EtcItem synthesized = new EtcItem(id, itemName, statData, templateItem.EtcItemgrp);
        _etcItems[id] = synthesized;
        return synthesized;
    }

    public AbstractItem GetItem(int id)
    {
        if (_weapons.TryGetValue(id, out Weapon weapon))
        {
            return weapon;
        }
        if (_armors.TryGetValue(id, out Armor armor))
        {
            return armor;
        }
        if (_etcItems.TryGetValue(id, out EtcItem etcItem))
        {
            return etcItem;
        }
        if (_hellsingCustomItems.TryGetValue(id, out var meta))
        {
            return meta.isWeapon ? (AbstractItem)GetWeapon(id) : GetArmor(id, meta.slot);
        }

        return GetEtcItem(id);
    }
}
