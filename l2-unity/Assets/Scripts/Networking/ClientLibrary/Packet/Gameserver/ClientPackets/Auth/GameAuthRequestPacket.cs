public class GameAuthRequestPacket : ClientPacket
{
    public GameAuthRequestPacket(string account, int playKey1, int playKey2, int loginKey1, int loginKey2) : base((byte)GameClientPacketType.AuthRequest)
    {
        WriteS(account);
        WriteI(playKey2);
        WriteI(playKey1);
        WriteI(loginKey1);
        WriteI(loginKey2);
        WriteI(1);

        BuildPacket();
    }
}
