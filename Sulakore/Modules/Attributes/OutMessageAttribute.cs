namespace Sulakore.Modules
{
    public sealed class OutMessageAttribute : MessageAttribute
    {
        public OutMessageAttribute(string identifier)
            : base(identifier, true)
        { }
    }
}