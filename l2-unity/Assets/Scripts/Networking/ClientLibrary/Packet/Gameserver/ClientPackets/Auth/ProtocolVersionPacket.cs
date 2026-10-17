public class ProtocolVersionPacket : ClientPacket
{
    public ProtocolVersionPacket(int version) : base((byte)GameClientPacketType.ProtocolVersion) {
        WriteI(version == 740 ? 746 : version);

        BuildPacket();
    }
}
