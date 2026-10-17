public enum LoginClientPacketType : byte {
    AuthRequest = 0x00,
    RequestServerLogin = 0x02,
    RequestServerList = 0x05,
    RequestGGAuth = 0x07,
    Ping = 0x99
}
