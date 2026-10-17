public class RequestCharSelectPacket : ClientPacket
{
    public RequestCharSelectPacket(int slot) : base((byte)GameClientPacketType.RequestCharSelect)
    {
        WriteI(slot);
        WriteB(0);
        WriteB(0);
        WriteI(0);
        WriteI(0);
        WriteI(0);
        BuildPacket();
    }
}
