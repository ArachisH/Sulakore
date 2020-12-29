namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the possible disconnection reasons.
    /// </summary>
    public enum HDisconnectReason
    {
        NotDisconnected = -2147483647,
        Unknown = -1,
        MaintenanceBreak = -2,
        Disconnected = -3,
        ConnectionFailed = -4,
        SocketDisconnect = -5,
        SocketTimeout = -6,
        ClientClosing = -7,
        ReselectServer = -8,
        ChangeAvatar = -9,
        CreateAvatar = -10,

        Logout = 0,
        JustBanned = 1,
        ConcurrentLogin = 2,
        ConnectionLostToPeer = 3,
        AvatarIdentityChange = 4,
        RemoveFurnitureTool = 5,

        StillBanned = 10,
        DualLoginByAvatarId = 11,
        HotelClosed = 12,
        DualLoginByIP = 13,

        PeerConnectionMissing = 16,
        NoLoginPermission = 17,
        DuplicateConnection = 18,
        HotelClosing = 19,
        IncorrectPassword = 20,
        InvalidToken = 21,
        InvalidLoginTicket = 22,

        VersionCheckUrl = 23,
        VersionCheckProperty = 24,
        VersionCheckMachineId = 25,

        NoMessengerSession = 26,
        UserNotFound = 27,
        CryptoNotInitializedOld = 28,
        DevCryptoNotAllowedOld = 29,

        CryptoNotInitialized = 50,
        DevCryptoNotAllowed = 51,
        CryptoReinitialization = 52,
        PublicKeyNotNumeric = 53,
        PublicKeyTooShort = 54,
        CryptoMitmAttack = 55,

        DuplicateUuidDetected = 100,
        OldSessionInProxy = 101,
        PublicKeyNotNumericOld = 102,
        PublicKeyTooShortOld = 103,
        SocketReadGeneric = 104,
        SocketReadFirstByte = 105,
        SocketReadLength = 106,
        SocketReadBody = 107,
        SocketReadPolicy = 108,
        SocketIOException = 109,
        SocketWrongCrypto = 110,

        ProxyRuntimeException = 111,
        IdleConnection = 112,
        PongTimeout = 113,
        IdleConnectionNotAuth = 114,
        IdleConnectionNoAvatarId = 115,
        WriteClosedChannel = 116,

        SocketWriteException1 = 117,
        SocketWriteException2 = 118,
        SocketWriteException3 = 119,

        TooManyBytesPendingWrite = 120,
        IdleConnectionPolicyRequest = 121,
        IncompatibleClientVersion = 122,
        CredentialsRemoved = 123,
        InsufficientSecurityLevel = 124,
        TooManyUndefinedClientMessages = 125,
        InvalidParameterRange = 126,
        ClientIpBlocked = 127,
        TooManyMessagesPendingWrite = 128,
        SocketWriteTimeout = 129,
        VersionUpdateRequired = 130
    }
}