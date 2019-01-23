using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Sulakore.Habbo.Messages
{
    public abstract class HMessages : IEnumerable<HMessage>
    {
        private readonly Dictionary<ushort, HMessage> _byId;
        private readonly Dictionary<string, HMessage> _byName, _byHash;

        public bool IsOutgoing { get; }

        public HMessage this[ushort id] => _byId[id];
        public HMessage this[string name] => _byName[name];

        public HMessages(bool isOutgoing)
            : this(isOutgoing, 0)
        { }
        public HMessages(bool isOutgoing, int capacity)
        {
            _byId = new Dictionary<ushort, HMessage>(capacity);
            _byName = new Dictionary<string, HMessage>(capacity);
            _byHash = new Dictionary<string, HMessage>(capacity);

            IsOutgoing = isOutgoing;
        }
        public HMessages(bool isOutgoing, IList<HMessage> messages)
            : this(isOutgoing, messages.Count)
        {
            foreach (HMessage message in messages)
            {
                _byId.Add(message.Id, message);
                _byHash.Add(message.Hash, message);
                if (!string.IsNullOrWhiteSpace(message.Name))
                {
                    _byName.Add(message.Name, message);

                    PropertyInfo property = GetType().GetProperty(message.Name);
                    property?.SetValue(this, message);
                }
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