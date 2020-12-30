namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the types of door modes possible.
    /// </summary>
    public enum HDoorMode
    {
        /// <summary>
        /// Represents a room that is open to everyone.
        /// </summary>
        Open = 0,
        /// <summary>
        /// Represents a room that requires ringing the doorbell to request access.
        /// </summary>
        Doorbell = 1,
        /// <summary>
        /// Represents a room that requires a password to enter.
        /// </summary>
        Password = 2,
        /// <summary>
        /// Represents a room that is invisible in the navigator.
        /// </summary>
        Invisible = 3,
        NoobsOnly = 4,
        Maintenance = 5,
        Frozen = 6,
        Friends = 7
    }
}
