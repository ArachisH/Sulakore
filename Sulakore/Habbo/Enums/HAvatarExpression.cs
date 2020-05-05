namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies avatar expressions a user can perform in the client.
    /// </summary>
    public enum HAvatarExpression
    {
        /// <summary>
        /// Represents a player without a gesture.
        /// </summary>
        None = 0,
        /// <summary>
        /// Represents a player waving.
        /// </summary>
        Wave = 1,
        /// <summary>
        /// Represents a player blowing a kiss. (HC Only)
        /// </summary>
        Blow = 2,
        /// <summary>
        /// Represents a player laughing. (HC Only)
        /// </summary>
        Laugh = 3,
        /// <summary>
        /// Represents a player crying.
        /// </summary>
        Cry = 4,
        /// <summary>
        /// Represents a player sleeping.
        /// </summary>
        Idle = 5,
        /// <summary>
        /// Represents a player hopping once. (HC Only)
        /// </summary>
        Jump = 6,
        /// <summary>
        /// Represents a player with raising a thumb up.
        /// </summary>
        ThumbsUp = 7
    }
}