namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the types of trade modes possible.
    /// </summary>
    public enum HTradeMode
    {
        /// <summary>
        /// Represents a room in which trading is not allowed.
        /// </summary>
        NotAllowed = 0,
        /// <summary>
        /// Represents a room in which only the owner and users with room rights are allowed to trade.
        /// </summary>
        Controller = 1,
        /// <summary>
        /// Represents a room in which everyone is allowed to trade.
        /// </summary>
        Allowed = 2
    }
}
