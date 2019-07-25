namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the possible disconnection reasons.
    /// </summary>
    public enum HDisconnectReason
    {
        Unknown = -1,
        MaintenanceBreak = -2,

        Logout = 0,
        JustBanned = 1,
        ConcurrentLogin = 2,
        ConnectionLostToPeer = 3,
        AvatarIdentityChange = 4,
        RemoveFurnitureTool = 5,
        StillBanned = 10,
        DualLoginById = 11,
        HotelClosed = 12,
        DualLoginByIP = 13,
        PeerConnectionMissing = 16,
        LoginNotAllowed = 17,
        DuplicateConnection = 18,
        HotelClosing = 19,
        IncorrectPassword = 20,
        InvalidLoginTicket = 22,

        VersionCheckUrl = 23,
        VersionCheckProperty = 24,
        VersionCheckMachineId = 25,

        NoMessengerSession = 26,
        UserNotFound = 27,
        CryptoNotInitialized = 28,
        DevCryptoNotAllowed = 29,
        DuplicateUniqueId = 100,
        OldSessionInProxy = 101,

        PublicKeyNotNumeric = 102,
        PublicKeyTooShort = 103,

        SocketReadGeneric = 104,
        SocketReadFirstByte = 105,
        SocketReadLength = 106,
        SocketReadBody = 107,
        SocketReadPolicy = 108,
        SocketIOException = 109,
        SocketWrongCrypto = 110,

        ProxyRuntimeException = 111,
        Idle = 112,
        PongTimeout = 113,
        IdleNotAuthenticated = 114,
        IdleNoUserId = 115,
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
        InvalidParameterRange = 126
    }
}