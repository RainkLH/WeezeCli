using System;
using System.Collections.Generic;
using System.Text;

namespace WeezeCli
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string description)
        {
            this.Description = description;
        }

        public string Description { get; }
    }
}
