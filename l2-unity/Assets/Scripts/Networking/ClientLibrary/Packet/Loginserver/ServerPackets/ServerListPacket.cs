using System.Collections.Generic;
using UnityEngine;

public class ServerListPacket : LoginServerPacket
{
    private byte _serverCount;
    private byte _lastServer;
    private List<ServerData> _serverData;
    private Dictionary<int, int> _charsOnServers;

    public byte LastServer { get { return _lastServer; } }
    public List<ServerData> ServersData { get { return _serverData; } }
    public Dictionary<int, int> CharsOnServers { get { return _charsOnServers; } }

    public class ServerData
    {
        public byte[] ip;
        public int port;
        public int currentPlayers;
        public int maxPlayers;
        public int status;
        public int serverId;

        public ServerData()
        {
            ip = new byte[4];
        }
    }

    public ServerListPacket(byte[] d) : base(d)
    {
        _serverData = new List<ServerData>();
        _charsOnServers = new Dictionary<int, int>();

        Parse();
    }

    public override void Parse()
    {
        _serverCount = ReadB();
        _lastServer = ReadB();

        for (int i = 0; i < _serverCount; i++)
        {
            ServerData serverData = new ServerData();
            serverData.serverId = ReadB();
            serverData.ip[0] = ReadB();
            serverData.ip[1] = ReadB();
            serverData.ip[2] = ReadB();
            serverData.ip[3] = ReadB();

            serverData.port = ReadI();
            byte ageLimit = ReadB();
            byte isPvp = ReadB();
            serverData.currentPlayers = ReadH();
            serverData.maxPlayers = ReadH();
            byte rawStatus = ReadB();
            int serverType = ReadI();
            byte brackets = ReadB();

            // In Interlude, rawStatus == 1 means Server Up ("Normal"/"Light"), 0 means "Down"
            serverData.status = rawStatus == 1 ? 1 : 4;

            Debug.Log($"Server data received: Id:{serverData.serverId} Ip:{StringUtils.ByteArrayToIpAddress(serverData.ip)} Port:{serverData.port} OnlineCount:{serverData.currentPlayers} MaxPlayers:{serverData.maxPlayers} Status:{serverData.status}");
            _serverData.Add(serverData);
            _charsOnServers[serverData.serverId] = 0;
        }
    }
}
