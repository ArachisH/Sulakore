using System;

using Sulakore.Habbo.Web;

namespace Sulakore.Habbo.Messages
{
    public abstract class Identifiers
    {
        private readonly IHGame _game;

        public abstract int Count { get; }
        public abstract bool IsOutgoing { get; }

        public bool IsUnity => _game?.IsUnity ?? true;

        public Identifiers()
            : this(null)
        { }
        public Identifiers(IHGame game)
        {
            _game = game;
        }

        public HMessage GetMessage(ushort id) => throw new NotImplementedException();
        public HMessage GetMessage(string name) => throw new NotImplementedException();

        protected HMessage Initialize(string name, short id)
        {
            if (!IsUnity)
            {
                id = _game.Resolve(name);
            }
            return new HMessage(name, IsOutgoing, (ushort)id);
        }
    }
}