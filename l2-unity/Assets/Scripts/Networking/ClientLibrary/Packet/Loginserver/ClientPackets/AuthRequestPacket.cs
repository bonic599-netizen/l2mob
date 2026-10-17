public class AuthRequestPacket : LoginClientPacket
{
    public AuthRequestPacket(byte[] rsaBlock, int sessionId = 0) : base((byte)LoginClientPacketType.AuthRequest)
    {
        WriteB(rsaBlock);
        WriteI(sessionId);
        WriteI(0x00000123);
        WriteI(0);
        WriteI(0);
        WriteI(0);
        WriteI(0x08);
        BuildPacket();
    }
}

public class RequestGGAuthPacket : LoginClientPacket
{
    public RequestGGAuthPacket(int sessionId) : base((byte)LoginClientPacketType.RequestGGAuth)
    {
        WriteI(sessionId);
        WriteI(0);
        WriteI(0);
        WriteI(0);
        WriteI(0);
        BuildPacket();
    }
}