public class InventoryUpdatePacket : AbstractItemPacket
{
    private ItemInstance[] _items;
    public ItemInstance[] Items { get { return _items; } }

    public InventoryUpdatePacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            int itemListSize = ReadH();

            var list = new System.Collections.Generic.List<ItemInstance>(itemListSize);
            var usedPaperdoll = new System.Collections.Generic.HashSet<Paperdoll>();
            int invSlot = 0;
            for (int i = 0; i < itemListSize; i++)
            {
                int lastChange = ReadH();
                ItemInstance item = ReadItem(invSlot, usedPaperdoll);
                item.LastChange = lastChange;
                if (!item.Equipped)
                {
                    invSlot++;
                }
                list.Add(item);
            }
            _items = list.ToArray();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[InventoryUpdatePacket] Parse error: {e}");
            if (_items == null) _items = System.Array.Empty<ItemInstance>();
        }
    }
}