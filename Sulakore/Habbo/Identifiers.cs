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

        protected HMessage Initialize(short id, string name)
        {
            var message = new HMessage(id, name, IsOutgoing);

            _byId.Add(id, message);
            _byName.Add(name, message);
            _messages[_byId.Count - 1] = message;
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