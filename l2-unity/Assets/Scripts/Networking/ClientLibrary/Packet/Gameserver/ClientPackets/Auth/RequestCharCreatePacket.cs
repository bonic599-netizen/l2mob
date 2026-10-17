public class RequestCharCreatePacket : ClientPacket
{
    public RequestCharCreatePacket(string name, CharacterRace race, CharacterSex sex, CharacterClass characterClass, int hairstyle, int haircolor, int face) : base((byte)GameClientPacketType.RequestCharCreate)
    {
        WriteS(name);
        WriteI((int)race);
        WriteI((int)sex);
        WriteI((int)characterClass);
        WriteI(21); // INT
        WriteI(40); // STR
        WriteI(43); // CON
        WriteI(25); // MEN
        WriteI(30); // DEX
        WriteI(11); // WIT
        WriteI(hairstyle);
        WriteI(haircolor);
        WriteI(face);
        BuildPacket();
    }
}
