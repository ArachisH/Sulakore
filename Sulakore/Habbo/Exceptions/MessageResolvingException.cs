namespace Sulakore.Habbo;

public sealed class MessageResolvingException : Exception
{
    public string Revision { get; }
    public string Name { get; }

    public MessageResolvingException(string revision, string name)
        : base($"Failed to resolve message \"{name}\" for revision {revision}.")
    {
        Revision = revision;
        Name = name;
    }
}