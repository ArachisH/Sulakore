using System.Collections;

namespace Sulakore.Habbo;

public abstract class Identifiers : IReadOnlyList<HMessage>
{
    private readonly HMessage[] _messages;
    private readonly Dictionary<short, HMessage> _byId;
    private readonly Dictionary<uint, HMessage> _byHash;
    private readonly Dictionary<string, HMessage> _byName;

    public bool IsOutgoing { get; }

    public HMessage this[short id] => _byId[id];
    public HMessage this[uint hash] => _byHash[hash];
    public HMessage this[string name] => _byName[name];

    public Identifiers(int count, bool isOutgoing)
    {
        IsOutgoing = isOutgoing;

        _messages = new HMessage[count];
        _byId = new Dictionary<short, HMessage>(count);
        _byHash = new Dictionary<uint, HMessage>(count);
        _byName = new Dictionary<string, HMessage>(count);
    }

    public bool TryGetMessage(short id, out HMessage message) => _byId.TryGetValue(id, out message);
    public bool TryGetMessage(uint hash, out HMessage message) => _byHash.TryGetValue(hash, out message);
    public bool TryGetMessage(string name, out HMessage message) => _byName.TryGetValue(name, out message);

    protected virtual void Register(HMessage message, string propertyName, ref HMessage backingField)
    {
        backingField = new HMessage(propertyName, message.Id, message.Hash, message.Structure, message.IsOutgoing, message.TypeName, message.ParserTypeName, message.References);

        _byId.Add(backingField.Id, backingField);
        _byName.Add(propertyName, backingField);
        if (!string.IsNullOrWhiteSpace(message.Name))
        {
            _byName.Add(message.Name, backingField);
        }
        if (message.Hash != 0)
        {
            _byHash.Add(message.Hash, backingField);
        }
    }
    protected virtual HMessage ResolveMessage(IGame game, string name, short unityId, string unityStructure, params uint[] postShuffleHashes)
    {
        HMessage message = default;
        if (!game.IsUnity)
        {
            for (int i = 0; i < postShuffleHashes.Length; i++)
            {
                if (game.TryResolveMessage(name, postShuffleHashes[i], IsOutgoing, out message)) break;
            }
        }
        else if (unityId > 0) message = new HMessage(name, unityId, 0, unityStructure, IsOutgoing, null, null, 0);

        if (message != default)
        {
            _byId.Add(message.Id, message);
            if (!game.IsUnity)
            {
                _byHash.Add(message.Hash, message);
            }
        }

        _byName.Add(name, message);
        _messages[_byName.Count - 1] = message;
        return message;
    }

    #region IReadOnlyList<HMessage> Implementation
    public int Count => _messages.Length;
    HMessage IReadOnlyList<HMessage>.this[int index] => _messages[index];

    IEnumerator IEnumerable.GetEnumerator() => _messages.GetEnumerator();
    public IEnumerator<HMessage> GetEnumerator() => ((IEnumerable<HMessage>)_messages).GetEnumerator();
    #endregion
}