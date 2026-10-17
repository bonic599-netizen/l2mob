using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LoginServerPacketHandler : ServerPacketHandler
{
    public override void HandlePacket(byte[] data)
    {
        LoginServerPacketType packetType = (LoginServerPacketType)data[0];
        if (LoginClient.Instance.LogReceivedPackets)
        {
            Debug.Log("[" + Thread.CurrentThread.ManagedThreadId + "] [LoginServer] Received packet:" + packetType);
        }

        switch (packetType)
        {
            case LoginServerPacketType.Ping:
                OnPingReceive();
                break;
            case LoginServerPacketType.Init:
                OnInitReceive(data);
                break;
            case LoginServerPacketType.LoginFail:
                OnLoginFail(data);
                break;
            case LoginServerPacketType.AccountKicked:
                OnAccountKicked(data);
                break;
            case LoginServerPacketType.LoginOk:
                OnLoginOk(data);
                break;
            case LoginServerPacketType.ServerList:
                OnServerListReceived(data);
                break;
            case LoginServerPacketType.PlayFail:
                OnPlayFail(data);
                break;
            case LoginServerPacketType.PlayOk:
                OnPlayOk(data);
                break;
            case LoginServerPacketType.GGAuth:
                OnGGAuth(data);
                break;
        }
    }

    protected override byte[] DecryptPacket(byte[] data)
    {
        if (LoginClient.Instance.LogCryptography)
        {
            Debug.Log("<---- [LOGIN] ENCRYPTED: " + StringUtils.ByteArrayToString(data));
        }

        LoginClient.Instance.DecryptBlowFish.processBigBlock(data, 0, data, 0, data.Length);

        if (LoginClient.Instance.LogCryptography)
        {
            Debug.Log("<---- [LOGIN] DECRYPTED: " + StringUtils.ByteArrayToString(data));
        }

        return data;
    }

    private void OnPingReceive()
    {
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        int ping = _timestamp != 0 ? (int)(now - _timestamp) : 0;
        //Debug.Log("Ping: " + ping + "ms");
        LoginClient.Instance.Ping = ping;

        Task.Delay(1000).ContinueWith(t =>
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                ((LoginClientPacketHandler)_clientPacketHandler).SendPing();
                _timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }

            Task.Delay(LoginClient.Instance.ConnectionTimeoutMs + 100).ContinueWith(t =>
            {
                if (!_tokenSource.IsCancellationRequested)
                {
                    long now2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (now2 - _timestamp >= LoginClient.Instance.ConnectionTimeoutMs)
                    {
                        Debug.Log("Connection timed out");
                        _client.Disconnect();
                    }
                }
            }, _tokenSource.Token);
        }, _tokenSource.Token);
    }

    private void OnInitReceive(byte[] data)
    {
        Debug.Log("On init receive");
        InitPacket packet = new InitPacket(data);

        byte[] rsaKey = packet.PublicKey;
        byte[] blowfishKey = packet.BlowfishKey;

        LoginClient.Instance.SessionKey1 = 0;
        LoginClient.Instance.SessionKey2 = 0;
        GameClient.Instance.SessionKey1 = 0;
        GameClient.Instance.SessionKey2 = 0;

        LoginClient.Instance.SetRSAKey(rsaKey);
        LoginClient.Instance.SetBlowFishKey(blowfishKey);

        _client.InitPacket = false;

        int sessionId = packet.SessionId;
        EventProcessor.Instance.QueueEvent(() => ((LoginClientPacketHandler)_clientPacketHandler).SendRequestGGAuth(sessionId));
    }

    private void OnGGAuth(byte[] data)
    {
        int ggSessionId = data.Length >= 5 ? BitConverter.ToInt32(data, 1) : 0;
        Debug.Log($"OnGGAuth received: sessionId={ggSessionId:X8}");
        EventProcessor.Instance.QueueEvent(() => ((LoginClientPacketHandler)_clientPacketHandler).SendAuth(ggSessionId));
    }

    private void OnLoginFail(byte[] data)
    {
        LoginServerFailPacket packet = new LoginServerFailPacket(data);

        LoginServerFailPacket.LoginFailedReason failedReason = packet.FailedReason;

        EventProcessor.Instance.QueueEvent(() =>
        {
            int systemMessageId = 449;
            switch (packet.FailedReason)
            {
                case LoginServerFailPacket.LoginFailedReason.REASON_USER_OR_PASS_WRONG:
                    systemMessageId = 449;
                    break;
                case LoginServerFailPacket.LoginFailedReason.REASON_ACCOUNT_IN_USE:
                    systemMessageId = 455;
                    break;
                case LoginServerFailPacket.LoginFailedReason.REASON_ACCESS_FAILED:
                    systemMessageId = 461;
                    break;
                case LoginServerFailPacket.LoginFailedReason.REASON_INACTIVE:
                    systemMessageId = 1920;
                    break;
            }
            L2ConfirmWindow.Instance.ShowWindow(systemMessageId, () =>
            {
            }, null);
        });

        Debug.LogWarning($"Login failed reason: {Enum.GetName(typeof(LoginServerFailPacket.LoginFailedReason), failedReason)}");

        _client.Disconnect();
    }

    private void OnAccountKicked(byte[] data)
    {
        AccountKickedPacket packet = new AccountKickedPacket(data);

        AccountKickedPacket.AccountKickedReason kickedReason = packet.KickedReason;

        Debug.LogWarning($"Account kicked reason: {Enum.GetName(typeof(AccountKickedPacket.AccountKickedReason), kickedReason)}");

        _client.Disconnect();
    }

    private void OnLoginOk(byte[] data)
    {
        LoginOkPacket packet = new LoginOkPacket(data);

        LoginClient.Instance.SessionKey1 = packet.SessionKey1;
        LoginClient.Instance.SessionKey2 = packet.SessionKey2;

        GameClient.Instance.SessionKey1 = packet.SessionKey1;
        GameClient.Instance.SessionKey2 = packet.SessionKey2;

        EventProcessor.Instance.QueueEvent(() => LoginClient.Instance.OnAuthAllowed());
    }

    private void OnServerListReceived(byte[] data)
    {
        ServerListPacket packet = new ServerListPacket(data);

        EventProcessor.Instance.QueueEvent(() =>
        {
            if (GameManager.Instance.State == GameState.LOGIN_CONNECTED)
            {
                LoginClient.Instance.OnAuthAllowed();
            }
            LoginClient.Instance.OnServerListReceived(packet);
        });
    }

    private void OnPlayFail(byte[] data)
    {
        PlayFailPacket packet = new PlayFailPacket(data);

        PlayFailPacket.PlayFailReason failedReason = packet.FailedReason;

        Debug.LogWarning($"Play failed reason: {Enum.GetName(typeof(PlayFailPacket.PlayFailReason), failedReason)}");
    }


    private void OnPlayOk(byte[] data)
    {
        LoginOkPacket packet = new LoginOkPacket(data);

        GameClient.Instance.PlayKey1 = packet.SessionKey1;
        GameClient.Instance.PlayKey2 = packet.SessionKey2;

        Debug.Log("Server select allowed.");

        EventProcessor.Instance.QueueEvent(() => LoginClient.Instance.OnPlayOk());
    }
}
