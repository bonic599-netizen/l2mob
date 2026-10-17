
using UnityEngine;

public abstract class AbstractItemPacket : ServerPacket
{
    protected AbstractItemPacket(byte[] d) : base(d)
    {
    }

    public ItemType1 Type1 { get; private set; }
    public int ObjectId { get; private set; }
    public int ItemId { get; private set; }
    public int Count { get; private set; }
    public int CustomType1 { get; private set; }
    public int CustomType2 { get; private set; }
    public int AugmentId { get; private set; }
    public ItemType2 Type2 { get; private set; }
    public bool Equipped { get; private set; }
    public ItemSlot BodyPart { get; private set; }
    public int EnchantLevel { get; private set; }
    public long RemainingTime { get; private set; }
    public ItemLocation ItemLocation { get; private set; }
    public int Slot { get; private set; }

    protected ItemInstance ReadItem(int defaultInventorySlot = 0, System.Collections.Generic.HashSet<Paperdoll> usedPaperdollSlots = null)
    {
        Type1 = (ItemType1)ReadH();
        ObjectId = ReadI();
        ItemId = ReadI();
        Count = ReadI();
        Type2 = (ItemType2)ReadH();
        CustomType1 = ReadH(); //NOT USED
        Equipped = ReadH() == 1;
        BodyPart = (ItemSlot)ReadI();
        EnchantLevel = ReadH();
        CustomType2 = ReadH(); //NOT USED
        AugmentId = ReadI(); //NOT USED
        RemainingTime = ReadI();

        ItemLocation = ItemLocation.Inventory;
        if (Equipped)
        {
            ItemLocation = ItemLocation.Equipped;
            Paperdoll pd = MapBodyPartToPaperdoll(BodyPart, Type1, Type2, usedPaperdollSlots);
            if (usedPaperdollSlots != null && pd != Paperdoll.NULL)
            {
                usedPaperdollSlots.Add(pd);
            }
            Slot = (int)pd;
        }
        else
        {
            Slot = defaultInventorySlot;
        }

        return new ItemInstance(
            ObjectId, ItemId, ItemLocation, Slot, Count, Type1, Type2,
            Equipped, BodyPart, EnchantLevel, RemainingTime);
    }

    private static Paperdoll MapBodyPartToPaperdoll(ItemSlot bodyPart, ItemType1 type1, ItemType2 type2, System.Collections.Generic.HashSet<Paperdoll> usedSlots)
    {
        int bp = (int)bodyPart;
        if (bp == 0x0001) return Paperdoll.UNDER;
        if ((bp & 0x0006) != 0)
        {
            if (usedSlots != null && usedSlots.Contains(Paperdoll.REAR)) return Paperdoll.LEAR;
            return Paperdoll.REAR;
        }
        if (bp == 0x0008) return Paperdoll.NECK;
        if ((bp & 0x0030) != 0)
        {
            if (usedSlots != null && usedSlots.Contains(Paperdoll.RFINGER)) return Paperdoll.LFINGER;
            return Paperdoll.RFINGER;
        }
        if (bp == 0x0040 || bp == 0x010000 || bp == 0x040000 || bp == 0x080000) return Paperdoll.HEAD;
        if (bp == 0x0080) return Paperdoll.RHAND;
        if (bp == 0x0100)
        {
            if (type2 == ItemType2.TYPE2_SHIELD_ARMOR || type1 == ItemType1.TYPE1_ITEM_QUESTITEM_ADENA)
                return Paperdoll.LHAND;
            return Paperdoll.RHAND;
        }
        if (bp == 0x4000)
        {
            if (type2 == ItemType2.TYPE2_WEAPON)
                return Paperdoll.RHAND;
            return Paperdoll.LHAND;
        }
        if (bp == 0x0200) return Paperdoll.GLOVES;
        if (bp == 0x0400 || bp == 0x8000 || bp == 0x020000) return Paperdoll.CHEST;
        if (bp == 0x0800) return Paperdoll.LEGS;
        if (bp == 0x1000) return Paperdoll.FEET;
        return Paperdoll.RHAND;
    }

    public override string ToString()
    {
        return $"Item Information:\n" +
               $"  Type1: {Type1}\n" +
               $"  Object ID: {ObjectId}\n" +
               $"  Item ID: {ItemId}\n" +
               $"  Slot: {Slot}\n" +
               $"  Count: {Count}\n" +
               $"  Type2: {Type2}\n" +
               $"  CustomType2: {CustomType1}\n" +
               $"  Equipped: {Equipped}\n" +
               $"  Body Part: {BodyPart}\n" +
               $"  Enchant Level: {EnchantLevel}\n" +
               $"  CustomType2: {CustomType2}\n" +
               $"  Augment Id: {AugmentId}\n" +
               $"  Remaining Time: {RemainingTime}\n" +
               $"  Item Location: {ItemLocation}\n";
    }
}
