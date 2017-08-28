using System;

namespace Sulakore.Modules
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AuthorAttribute : Attribute
    {
        public string Name { get; }

        public AuthorAttribute(string name)
        {
            Name = name;
        }
    }
}