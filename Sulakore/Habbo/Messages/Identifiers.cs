using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Sulakore.Habbo.Messages
{
    public abstract class Identifiers : IEnumerable<HMessage>
    {
        private readonly Dictionary<ushort, HMessage> _byId;
        private readonly Dictionary<string, HMessage> _byName, _byHash;

        public bool IsOutgoing { get; }
        public int Count => _byId.Count;

        public HMessage this[ushort id] => GetMessage(id);
        public HMessage this[string identifier] => GetMessage(identifier);

        public Identifiers(bool isOutgoing)
            : this(isOutgoing, 0)
        { }
        public Identifiers(bool isOutgoing, int capacity)
        {
            _byId = new Dictionary<ushort, HMessage>(capacity);
            _byName = new Dictionary<string, HMessage>(capacity);
            _byHash = new Dictionary<string, HMessage>(capacity);

            IsOutgoing = isOutgoing;
        }
        public Identifiers(bool isOutgoing, IList<HMessage> messages)
            : this(isOutgoing, messages.Count)
        {
            foreach (HMessage message in messages)
            {
                _byId.Add(message.Id, message);
                _byHash.TryAdd(message.Hash, message); //TODO:
                if (!string.IsNullOrWhiteSpace(message.Name))
                {
                    _byName.Add(message.Name, message);

                    PropertyInfo property = GetType().GetProperty(message.Name);
                    property?.SetValue(this, message);
                }
            }
        }

        public void Remove(HMessage message)
        {
            _byId.Remove(message.Id);
            if (string.IsNullOrWhiteSpace(message.Hash))
            {
                _byHash.Remove(message.Hash);
            }
            if (!string.IsNullOrWhiteSpace(message.Name))
            {
                _byName.Remove(message.Name);
                GetType().GetProperty(message.Name)?.SetValue(this, null);
            }
        }
        public void AddOrUpdate(HMessage message)
        {
            _byId.TryAdd(message.Id, message);
            if (!string.IsNullOrWhiteSpace(message.Name))
            {
                _byName.TryAdd(message.Name, message);
                GetType().GetProperty(message.Name).SetValue(this, message);
            }
            if (!string.IsNullOrWhiteSpace(message.Hash))
            {
                _byHash.TryAdd(message.Hash, message);
            }
        }

        public HMessage GetMessage(ushort id)
        {
            _byId.TryGetValue(id, out HMessage message);
            return message;
        }
        public HMessage GetMessage(string identifier)
        {
            if (_byHash.TryGetValue(identifier, out HMessage namedMessage)) return namedMessage;
            if (_byName.TryGetValue(identifier, out HMessage hashedMessage)) return hashedMessage;
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => _byId.Values.GetEnumerator();
        IEnumerator<HMessage> IEnumerable<HMessage>.GetEnumerator() => _byId.Values.GetEnumerator();
    }
}