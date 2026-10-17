using System;

public class InventoryItemListPacket : AbstractItemPacket
{
    private ItemInstance[] _items;
    public ItemInstance[] Items { get { return _items; } }

    private bool _openWindow;
    public Boolean OpenWindow { get { return _openWindow; } }

    public InventoryItemListPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            _openWindow = ReadH() == 1;
            int itemListSize = ReadH();

            var list = new System.Collections.Generic.List<ItemInstance>(itemListSize);
            var usedPaperdoll = new System.Collections.Generic.HashSet<Paperdoll>();
            int invSlot = 0;
            for (int i = 0; i < itemListSize; i++)
            {
                ItemInstance item = ReadItem(invSlot, usedPaperdoll);
                if (!item.Equipped)
                {
                    invSlot++;
                }
                list.Add(item);
            }
            _items = list.ToArray();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[InventoryItemListPacket] Parse error: {e}");
            if (_items == null) _items = Array.Empty<ItemInstance>();
        }
    }
}
