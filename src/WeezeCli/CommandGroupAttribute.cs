using System;
using System.Collections.Generic;
using System.Text;

namespace WeezeCli
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandGroupAttribute : Attribute
    {
        public CommandGroupAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
        public CommandGroupAttribute(string description) : this(Config.DefaultGroupName, description) { }

        public string Name { get; }

        public string Description { get; }
    }
}
