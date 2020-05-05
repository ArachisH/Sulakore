namespace Sulakore.Habbo
{
    /// <summary>
    /// Specifies the types of entities that can be in a room.
    /// </summary>
    public enum HEntityType
    {
        /// <summary>
        /// Represents a regular user entity.
        /// </summary>
        User = 1,
        /// <summary>
        /// Represents a pet entity.
        /// </summary>
        Pet = 2,
        /// <summary>
        /// Represents a regular room bot.
        /// </summary>
        Bot = 3,
        /// <summary>
        /// Represents a rentable bot which can be bought from the catalog.
        /// </summary>
        RentableBot = 4
    }
}
