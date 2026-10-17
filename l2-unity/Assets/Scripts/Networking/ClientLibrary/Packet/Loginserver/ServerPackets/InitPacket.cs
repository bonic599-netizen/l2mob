using UnityEngine;
class InitPacket : LoginServerPacket
{
    private int sessionId;
    private int rsaKeyLength;
    private byte[] publicKey;
    private byte[] blowfishKey;

    public int SessionId { get { return sessionId; } }
    public int RSAKeyLength { get { return rsaKeyLength; } }
    public byte[] PublicKey { get { return publicKey; } }
    public byte[] BlowfishKey { get { return blowfishKey; } }

    public InitPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        sessionId = ReadI();
        int protocolRevision = ReadI();
        rsaKeyLength = 128;
        publicKey = ReadB(128);
        ReadB(16); // GameGuard bytes
        blowfishKey = ReadB(16);

        Debug.Log($"InitPacket: sessionId [{sessionId:X8}], protocol [{protocolRevision:X8}]");
    }
}