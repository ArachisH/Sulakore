using System.Collections;
using System.Collections.Generic;

namespace Sulakore.Habbo
{
    public abstract class Identifiers : IReadOnlyList<HMessage>
    {
        private readonly HMessage[] _messages;
        private readonly Dictionary<short, HMessage> _byId;
        private readonly Dictionary<string, HMessage> _byName;

        public bool IsOutgoing { get; }

        public HMessage this[short id] => _byId[id];
        public HMessage this[string name] => _byName[name];

        public Identifiers(int count, bool isOutgoing)
        {
            IsOutgoing = isOutgoing;

            _messages = new HMessage[count];
            _byId = new Dictionary<short, HMessage>(count);
            _byName = new Dictionary<string, HMessage>(count);
        }

        public bool TryGetMessage(short id, out HMessage message) => _byId.TryGetValue(id, out message);
        public bool TryGetMessage(string name, out HMessage message) => _byName.TryGetValue(name, out message);

        protected HMessage Initialize(short id, string name)
        {
            // TODO: Use short everywhere.
            var message = new HMessage((ushort)id, name, IsOutgoing);
            if (id != -1)
            {
                _byId.Add(id, message);
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
}