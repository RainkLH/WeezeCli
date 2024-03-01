using System;
using System.Collections.Generic;
using System.Text;

namespace WeezeCli.Model
{
    internal class CommandGroup
    {
        public CommandGroup(string name, string description, object obj)
        {
            this.Name = name;
            this.Description = description;
            this.Instance = obj;
        }
        public string Name { get; }

        public string Description { get; }

        public object Instance { get; }
    }

}
