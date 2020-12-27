namespace Sulakore.Habbo.Packages
{
    /// <summary>
    /// Represents a single moving object in the room.
    /// </summary>
    public class HSlideObject
    {
        /// <summary>
        /// Represents the unique identifier of the sliding object. 
        /// For moving entities the unique identifier represents their current <see cref="HEntity.Index"/> in the room.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The start location from which the movement starts.
        /// </summary>
        public HPoint Location { get; set; }
        /// <summary>
        /// The final location of the moving object.
        /// </summary>
        public HPoint Target { get; set; }
        public HMoveType Type { get; set; }

        public HSlideObject(int id, HPoint location, HPoint target, HMoveType type = HMoveType.None)
        {
            Id = id;
            Location = location;
            Target = target;
            Type = type;
        }
    }
}
