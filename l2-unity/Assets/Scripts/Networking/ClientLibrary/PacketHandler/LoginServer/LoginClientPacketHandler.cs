using System;
using System.Text;
using System.Threading;
using UnityEngine;

public class LoginClientPacketHandler : ClientPacketHandler
{
    protected override void EncryptPacket(ClientPacket packet)
    {
        base.EncryptPacket(packet);

        byte[] data = packet.GetData();

        LoginClient.Instance.EncryptBlowFish.processBigBlock(data, 0, data, 0, data.Length);

        if (LoginClient.Instance.LogCryptography)
        {
            Debug.Log("----> [LOGIN] ENCRYPTED: " + StringUtils.ByteArrayToString(data));
        }

        packet.SetData(data);
    }

    public void SendPing()
    {
        PingPacket packet = new PingPacket();
        SendPacket(packet);
    }

    public void SendRequestGGAuth(int sessionId)
    {
        RequestGGAuthPacket packet = new RequestGGAuthPacket(sessionId);
        SendPacket(packet);
    }

    public void SendAuth(int sessionId = 0)
    {
        string account = LoginClient.Instance.Account ?? "";
        string password = LoginClient.Instance.Password ?? "";

        byte[] accountBytes = Encoding.ASCII.GetBytes(account);
        byte[] passBytes = Encoding.ASCII.GetBytes(password);

        byte[] rsaBlock = new byte[128];
        rsaBlock[0x5B] = 0x24;
        Array.Copy(accountBytes, 0, rsaBlock, 0x5E, Math.Min(14, accountBytes.Length));
        Array.Copy(passBytes, 0, rsaBlock, 0x6C, Math.Min(16, passBytes.Length));

        rsaBlock = LoginClient.Instance.RSACrypt.EncryptRSANoPadding(rsaBlock);

        AuthRequestPacket packet = new AuthRequestPacket(rsaBlock, sessionId);

        SendPacket(packet);
    }

    public void SendRequestServerList()
    {
        RequestServerListPacket packet = new RequestServerListPacket(LoginClient.Instance.SessionKey1, LoginClient.Instance.SessionKey2);

        SendPacket(packet);
    }

    public void SendRequestServerLogin(int serverId)
    {
        RequestServerLoginPacket packet = new RequestServerLoginPacket(serverId, LoginClient.Instance.SessionKey1, LoginClient.Instance.SessionKey2);

        SendPacket(packet);
    }

    public override void SendPacket(ClientPacket packet)
    {
        if (LoginClient.Instance.LogSentPackets)
        {
            LoginClientPacketType packetType = (LoginClientPacketType)packet.GetPacketType();
            Debug.Log("[" + Thread.CurrentThread.ManagedThreadId + "] [LoginServer] Sending packet:" + packetType);
        }

        if (LoginClient.Instance.LogCryptography)
        {
            Debug.Log("----> [LOGIN] CLEAR: " + StringUtils.ByteArrayToString(packet.GetData()));
        }

        EncryptPacket(packet);

        _client.SendPacket(packet);
    }
}
