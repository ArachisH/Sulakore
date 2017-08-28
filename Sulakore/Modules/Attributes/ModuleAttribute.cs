using System;

namespace Sulakore.Modules
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ModuleAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public Type EntryType { get; set; }
        public string PropertyName { get; set; }

        public ModuleAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}