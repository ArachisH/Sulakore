using System;
using System.Collections.Generic;

namespace Sulakore.Habbo
{
    public sealed class MessageResolvingException : Exception
    {
        public string Revision { get; }
        public IDictionary<string, IList<string>> Unresolved { get; }

        public MessageResolvingException(string revision, IDictionary<string, IList<string>> unresolved)
            : base($"Failed to resolve {unresolved.Count:n0} identifiers for {revision}.")
        {
            Revision = revision;
            Unresolved = unresolved;
        }
    }
}