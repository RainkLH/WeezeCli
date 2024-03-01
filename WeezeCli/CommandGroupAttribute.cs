using System;
using System.Collections.Generic;
using System.Text;

namespace WeezeCli
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandGroupAttribute : Attribute
    {
        public CommandGroupAttribute(string description, bool asMainApp = false)
        {
            this.Description = description;
            this.AsMainApp = asMainApp;
        }

        public bool AsMainApp { get; }

        public string Description { get; }
    }
}
