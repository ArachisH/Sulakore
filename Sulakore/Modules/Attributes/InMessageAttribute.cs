namespace Sulakore.Modules
{
    public sealed class InMessageAttribute : MessageAttribute
    {
        public InMessageAttribute(string identifier)
            : base(identifier, false)
        { }
    }
}